import { useState, useRef, useEffect } from 'react'
import { Bulb, Send, TargetDot, Check, Warning } from '../components/Icons.jsx'
import { evaluate } from '../evaluate.js'

const HINT = 'You could share a little more — try adding how you feel or a small personal detail. In the U.S. that reads as warm and open, not oversharing.'

export default function Chat({ actor, scenario, onEnd }) {
  const p = actor.persona
  const [messages, setMessages] = useState([{ who: 'ai', text: scenario.opener }])
  const [notes, setNotes] = useState([])
  const [turn, setTurn] = useState(0)
  const [goalPct, setGoalPct] = useState(20)
  const [draft, setDraft] = useState('')
  const [confirming, setConfirming] = useState(false)
  const msgsRef = useRef(null)
  const userTurns = messages.filter((m) => m.who === 'me').length

  function finish() {
    onEnd(evaluate(messages, p.name), messages)
  }

  useEffect(() => {
    if (msgsRef.current) msgsRef.current.scrollTop = msgsRef.current.scrollHeight
  }, [messages])

  function send() {
    const v = draft.trim()
    if (!v) return
    setMessages((m) => [...m, { who: 'me', text: v }])
    setDraft('')
    setGoalPct(Math.min(20 + (turn + 1) * 22, 100))
    setNotes((n) => [...n, v.length < 25
      ? { kind: 'try', text: 'Short answers can feel distant here. Try adding a small personal detail.' }
      : { kind: 'good', text: `Nice — you gave enough for ${p.name} to respond to.` }])
    const reply = scenario.replies[turn % scenario.replies.length]
    setTurn((t) => t + 1)
    setTimeout(() => setMessages((m) => [...m, { who: 'ai', text: reply }]), 650)
  }

  const hint = () => setMessages((m) => [...m, { who: 'tip', text: HINT }])

  return (
    <div className="chatscreen">
      <div className="chatcol">
        <div className="chattop">
          <div style={{ width: 44, height: 44, borderRadius: '50%', background: p.accent, color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontFamily: "'Bricolage Grotesque'", fontWeight: 600, fontSize: 18, flexShrink: 0 }}>{p.initial}</div>
          <div style={{ flexGrow: 1 }}>
            <div style={{ fontWeight: 600, fontSize: 16 }}>{p.name}</div>
            <div className="muted row" style={{ gap: 6, fontSize: 12.5 }}>
              <span style={{ width: 7, height: 7, borderRadius: '50%', background: 'var(--teal)', display: 'inline-block' }} />{p.role}
            </div>
          </div>
          <button className="btn-primary" style={{ padding: '10px 20px', fontSize: 14 }} onClick={() => setConfirming(true)}>
            <Check size={16} sw={2.6} />Done
          </button>
        </div>

        <div className="msgs" ref={msgsRef}>
          {messages.map((m, i) => {
            if (m.who === 'ai') return (
              <div className="aiwrap" key={i}><div className="mini" style={{ background: p.accent }}>{p.initial}</div><div className="bubble ai">{m.text}</div></div>
            )
            if (m.who === 'me') return (<div className="bubble me" key={i}>{m.text}</div>)
            return (
              <div className="tip" key={i}>
                <div className="row" style={{ gap: 7, fontSize: 12, fontWeight: 600, color: 'var(--amber)' }}><Bulb size={15} sw={2} />Gentle tip</div>
                <div style={{ fontSize: 13, color: '#6b5a30', marginTop: 4, lineHeight: 1.45 }}>{m.text}</div>
              </div>
            )
          })}
        </div>

        <div className="inputbar">
          <div className="row" style={{ gap: 12 }}>
            <button className="hintbtn" onClick={hint}><Bulb size={18} />Need a hint</button>
            <input className="field" placeholder="Type your reply…" value={draft}
              onChange={(e) => setDraft(e.target.value)}
              onKeyDown={(e) => { if (e.key === 'Enter') send() }} />
            <button className="sendbtn" onClick={send}><Send size={21} /></button>
          </div>
        </div>
      </div>

      <aside className="panel">
        <div>
          <div className="label" style={{ marginBottom: 11 }}>Your goal</div>
          <div style={{ background: 'var(--clay-tint)', border: '1px solid #F0DAC8', borderRadius: 14, padding: '15px 16px' }}>
            <div className="row" style={{ gap: 10 }}>
              <span style={{ color: 'var(--clay)', flexShrink: 0 }}><TargetDot size={20} /></span>
              <div style={{ fontSize: 14, fontWeight: 500 }}>{scenario.goal}</div>
            </div>
            <div style={{ height: 6, background: '#F0DAC8', borderRadius: 999, marginTop: 14 }}>
              <div style={{ width: `${goalPct}%`, height: 6, background: 'var(--clay)', borderRadius: 999, transition: 'width .4s' }} />
            </div>
            <div className="muted" style={{ fontSize: 12, marginTop: 7 }}>Keep the conversation going.</div>
          </div>
        </div>

        <div>
          <div className="label" style={{ marginBottom: 11 }}>Culture notes so far</div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
            {notes.length === 0 && (
              <div style={{ border: '1px solid var(--line)', borderRadius: 12, padding: '12px 14px', color: 'var(--ink-soft)', fontSize: 13 }}>
                Notes will appear here as you chat.
              </div>
            )}
            {notes.map((n, i) => {
              const good = n.kind === 'good'
              return (
                <div key={i} style={{ borderRadius: 12, padding: '12px 14px', ...(good ? { border: '1px solid var(--line)' } : { border: '1px solid #EAD9AE', background: 'var(--amber-tint)' }) }}>
                  <div className="row" style={{ gap: 8, fontSize: 13, fontWeight: 600, color: good ? 'var(--teal-d)' : 'var(--amber)' }}>
                    {good ? <Check size={15} sw={2.4} /> : <Warning size={15} />}{good ? 'Good move' : 'To try'}
                  </div>
                  <div style={{ fontSize: 13, color: good ? '#514a41' : '#6b5a30', marginTop: 4, lineHeight: 1.4 }}>{n.text}</div>
                </div>
              )
            })}
          </div>
        </div>

        <div style={{ marginTop: 'auto', fontSize: 11.5, color: 'var(--ink-soft)', lineHeight: 1.5, borderTop: '1px solid var(--line)', paddingTop: 14 }}>
          Bridge helps you practice communication skills. It is not a substitute for professional support.
        </div>
      </aside>

      {confirming && (
        <div className="overlay" onClick={() => setConfirming(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div style={{ width: 46, height: 46, borderRadius: 14, background: 'var(--teal-tint)', color: 'var(--teal-d)', display: 'flex', alignItems: 'center', justifyContent: 'center', marginBottom: 16 }}>
              <Check size={24} sw={2.4} />
            </div>
            <h2 style={{ fontSize: 21 }}>Finish this conversation?</h2>
            <p style={{ fontSize: 14.5, color: 'var(--ink-soft)', lineHeight: 1.5, marginTop: 8 }}>
              We'll review how the whole chat went with {p.name} and save it to your progress. You've spoken {userTurns} {userTurns === 1 ? 'time' : 'times'} so far.
            </p>
            <div className="row" style={{ gap: 10, marginTop: 22 }}>
              <button className="btn-ghost" style={{ flex: 1 }} onClick={() => setConfirming(false)}>Keep chatting</button>
              <button className="btn-primary" style={{ flex: 1, borderRadius: 14 }} onClick={finish}>Yes, review it</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
