import { useEffect, useRef, useState } from 'react'
import { Bulb, Send, TargetDot, Check, Warning } from '../components/Icons.jsx'
import { bridgeApi } from '../api.js'

function feedbackText(item) {
  if (typeof item === 'string') return item
  return [item.title, item.evidence, item.suggestion].filter(Boolean).join(' — ')
}

export default function Chat({ actor, scenario, session, onEnd }) {
  const persona = actor.persona
  const [messages, setMessages] = useState([{ who: 'ai', text: session.openingMessage }])
  const [notes, setNotes] = useState([])
  const [goalPct, setGoalPct] = useState(0)
  const [draft, setDraft] = useState('')
  const [confirming, setConfirming] = useState(false)
  const [sending, setSending] = useState(false)
  const [hinting, setHinting] = useState(false)
  const [finishing, setFinishing] = useState(false)
  const [error, setError] = useState('')
  const [hintCount, setHintCount] = useState(0)
  const msgsRef = useRef(null)
  const userTurns = messages.filter((message) => message.who === 'me').length

  useEffect(() => {
    if (msgsRef.current) msgsRef.current.scrollTop = msgsRef.current.scrollHeight
  }, [messages])

  async function send() {
    const content = draft.trim()
    if (!content || sending) return
    setDraft('')
    setError('')
    setSending(true)
    setMessages((current) => [...current, { who: 'me', text: content }])
    try {
      const result = await bridgeApi.sendMessage(session.id, content)
      if (result.reply) setMessages((current) => [...current, { who: 'ai', text: result.reply }])
      if (result.cultureNote) {
        setNotes((current) => [...current, {
          kind: result.cultureNoteType?.toLowerCase().includes('good') ? 'good' : 'try',
          text: result.cultureNote,
        }])
      }
      setGoalPct(result.goalProgress || 0)
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setSending(false)
    }
  }

  async function requestHint() {
    if (hinting) return
    setError('')
    setHinting(true)
    try {
      const result = await bridgeApi.getHint(session.id)
      setMessages((current) => [...current, { who: 'tip', text: result.hint }])
      setHintCount((count) => count + 1)
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setHinting(false)
    }
  }

  async function finish() {
    if (userTurns === 0 || finishing) {
      setError('Send at least one reply before finishing the conversation.')
      setConfirming(false)
      return
    }
    setError('')
    setFinishing(true)
    try {
      const result = await bridgeApi.completeSession(session.id)
      const learningPlan = await bridgeApi.getLearningPlan(session.id)
      onEnd({
        score: result.overallScore,
        dimensions: result.dimensions,
        strengths: (result.strengths || []).map(feedbackText),
        toTry: (result.improvements || []).map(feedbackText),
        cultureGap: result.cultureGap,
        summary: result.summary,
        weakestSkill: learningPlan.focusSkill,
        learningPlan,
        turns: userTurns,
        hints: hintCount,
      })
    } catch (requestError) {
      setError(requestError.message)
      setConfirming(false)
    } finally {
      setFinishing(false)
    }
  }

  return (
    <div className="chatscreen">
      <div className="chatcol">
        <div className="chattop">
          <div className="chat-avatar" style={{ background: persona.accent }}>{persona.initial}</div>
          <div className="chat-person">
            <div className="chat-name">{persona.name}</div>
            <div className="muted row chat-role"><span className="online-dot" />{persona.role}</div>
          </div>
          <button className="btn-primary done-button" onClick={() => setConfirming(true)} disabled={sending || finishing}>
            <Check size={16} sw={2.6} />Done
          </button>
        </div>

        {error && <div className="inline-error" role="alert">{error}</div>}

        <div className="msgs" ref={msgsRef}>
          {messages.map((message, index) => {
            if (message.who === 'ai') return (
              <div className="aiwrap" key={index}><div className="mini" style={{ background: persona.accent }}>{persona.initial}</div><div className="bubble ai">{message.text}</div></div>
            )
            if (message.who === 'me') return <div className="bubble me" key={index}>{message.text}</div>
            return <div className="tip" key={index}>
              <div className="row tip-label"><Bulb size={15} sw={2} />Gentle tip</div>
              <div className="tip-copy">{message.text}</div>
            </div>
          })}
          {sending && <div className="aiwrap" aria-label={`${persona.name} is replying`}><div className="mini" style={{ background: persona.accent }}>{persona.initial}</div><div className="typing"><span /><span /><span /></div></div>}
        </div>

        <div className="inputbar">
          <div className="row chat-input-row">
            <button className="hintbtn" onClick={requestHint} disabled={hinting || sending}><Bulb size={18} />{hinting ? 'Thinking' : 'Need a hint'}</button>
            <input className="field" placeholder="Type your reply…" value={draft} disabled={sending || finishing}
              onChange={(event) => setDraft(event.target.value)}
              onKeyDown={(event) => { if (event.key === 'Enter') send() }} />
            <button className="sendbtn" onClick={send} disabled={!draft.trim() || sending} aria-label="Send reply"><Send size={21} /></button>
          </div>
        </div>
      </div>

      <aside className="panel">
        <div>
          <div className="label panel-label">Your goal</div>
          <div className="goal-panel">
            <div className="row goal-copy"><TargetDot size={20} /><div>{scenario.goal}</div></div>
            <div className="progress-track"><div style={{ width: `${goalPct}%` }} /></div>
            <div className="muted progress-copy">{goalPct ? `${goalPct}% toward the goal` : 'Start with your first reply.'}</div>
          </div>
        </div>

        <div>
          <div className="label panel-label">Culture notes so far</div>
          <div className="notes-list">
            {notes.length === 0 && <div className="empty-note">Notes from the AI coach will appear here.</div>}
            {notes.map((note, index) => {
              const good = note.kind === 'good'
              return <div key={index} className={`culture-note ${good ? 'good' : 'try'}`}>
                <div className="row note-title">{good ? <Check size={15} sw={2.4} /> : <Warning size={15} />}{good ? 'Good move' : 'To try'}</div>
                <div className="note-copy">{note.text}</div>
              </div>
            })}
          </div>
        </div>

        <div className="panel-disclaimer">Bridge helps you practice communication skills. It is not a substitute for professional support.</div>
      </aside>

      {confirming && <div className="overlay" onClick={() => !finishing && setConfirming(false)}>
        <div className="modal" onClick={(event) => event.stopPropagation()}>
          <div className="modal-icon"><Check size={24} sw={2.4} /></div>
          <h2>Finish this conversation?</h2>
          <p>Bridge will evaluate the complete chat with {persona.name} and save the result to your progress. You have sent {userTurns} {userTurns === 1 ? 'reply' : 'replies'}.</p>
          <div className="row modal-actions">
            <button className="btn-ghost" onClick={() => setConfirming(false)} disabled={finishing}>Keep chatting</button>
            <button className="btn-primary" onClick={finish} disabled={finishing}>{finishing ? 'Reviewing…' : 'Yes, review it'}</button>
          </div>
        </div>
      </div>}
    </div>
  )
}