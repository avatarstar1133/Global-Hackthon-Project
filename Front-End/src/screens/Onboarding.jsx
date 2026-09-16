import { useEffect, useMemo, useState } from 'react'
import { bridgeApi } from '../api.js'
import { Bridge, ArrowRight, ChevronLeft, Sparkle, Check, Plus } from '../components/Icons.jsx'

const ASSESSMENT_ID = 'persona-v1'

export default function Onboarding({ userId, onComplete }) {
  const [sections, setSections] = useState(null)
  const [loadError, setLoadError] = useState('')
  const [i, setI] = useState(0)
  const [answers, setAnswers] = useState({}) // questionId -> { selectedValues:[], scaleValue }
  const [phase, setPhase] = useState('questions') // questions | submitting | review
  const [result, setResult] = useState(null)
  const [submitError, setSubmitError] = useState('')

  useEffect(() => {
    let active = true
    bridgeApi.getAssessment(ASSESSMENT_ID)
      .then((data) => { if (active) setSections(data.sections) })
      .catch((error) => { if (active) setLoadError(error.message) })
    return () => { active = false }
  }, [])

  // Flatten to one-question-per-step, keeping section context.
  const flat = useMemo(() => {
    if (!sections) return []
    return sections.flatMap((section, sectionIndex) =>
      section.questions.map((question) => ({ question, sectionTitle: section.title, sectionIndex })))
  }, [sections])

  if (loadError) {
    return <Centered><h1>We couldn't load the check-in</h1><p className="muted">{loadError}</p></Centered>
  }
  if (!sections || flat.length === 0) {
    return <Centered><div className="status-pulse" /><h1>Preparing your check-in…</h1></Centered>
  }

  if (phase === 'review') {
    return <Review result={result} answers={answers} flat={flat} onStart={() => onComplete(result.userId)} />
  }

  const { question, sectionTitle, sectionIndex } = flat[i]
  const answer = answers[question.id] || {}
  const isMulti = question.answerType === 'multi-select'

  function setAnswer(next) {
    setAnswers((current) => ({ ...current, [question.id]: next }))
  }

  function advance() {
    if (i < flat.length - 1) setI(i + 1)
    else submit()
  }

  function pickScale(value) {
    setAnswer({ scaleValue: value })
    setTimeout(advance, 160)
  }
  function pickSingle(value) {
    setAnswer({ selectedValues: [value] })
    setTimeout(advance, 160)
  }
  function toggleMulti(value) {
    const selected = answer.selectedValues || []
    const has = selected.includes(value)
    const next = has ? selected.filter((v) => v !== value) : [...selected, value]
    if (!has && next.length > question.maxSelections) return // respect the cap
    setAnswer({ selectedValues: next })
  }

  async function submit() {
    setPhase('submitting')
    setSubmitError('')
    try {
      let id = userId
      if (!id) {
        const user = await bridgeApi.createAnonymousUser('Learner')
        id = user.id
      }
      const payload = flat.map(({ question: q }) => {
        const a = answers[q.id] || {}
        return q.answerType === 'scale'
          ? { questionId: q.id, selectedValues: null, scaleValue: a.scaleValue ?? null }
          : { questionId: q.id, selectedValues: a.selectedValues ?? [], scaleValue: null }
      })
      const response = await bridgeApi.submitAssessment(ASSESSMENT_ID, id, payload)
      setResult({ ...response, userId: id })
      setPhase('review')
    } catch (error) {
      setSubmitError(error.message)
      setPhase('questions')
    }
  }

  const progress = Math.round(((i) / flat.length) * 100)
  const multiReady = isMulti && (answer.selectedValues?.length || 0) > 0

  return (
    <Centered>
      <div className="brand" style={{ padding: 0, justifyContent: 'center' }}>
        <span className="mark"><Bridge size={19} /></span>Lanco
      </div>

      <div style={{ height: 6, background: 'var(--line)', borderRadius: 999, overflow: 'hidden' }}>
        <div style={{ width: `${progress}%`, height: 6, background: 'var(--teal)', borderRadius: 999, transition: 'width .25s' }} />
      </div>
      <div className="row" style={{ justifyContent: 'space-between' }}>
        <span className="label" style={{ margin: 0 }}>Section {sectionIndex + 1} of {sections.length} · {sectionTitle}</span>
        {i > 0 && phase !== 'submitting' && (
          <button className="back" onClick={() => setI(i - 1)} style={{ padding: 0 }}><ChevronLeft size={16} />Back</button>
        )}
      </div>

      <h1 className="onboarding-question">{question.prompt}</h1>

      {question.answerType === 'scale' && (
        <div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5,1fr)', gap: 8 }}>
            {Array.from({ length: (question.scaleMax || 5) - (question.scaleMin || 1) + 1 }, (_, k) => {
              const value = (question.scaleMin || 1) + k
              const on = answer.scaleValue === value
              return (
                <button key={value} className="card" onClick={() => pickScale(value)}
                  style={{ padding: '16px 6px', display: 'flex', justifyContent: 'center', borderColor: on ? 'var(--teal)' : 'var(--line)', background: on ? 'var(--teal-tint)' : 'var(--surface)' }}>
                  <span style={{ fontFamily: "'Bricolage Grotesque'", fontWeight: 700, fontSize: 20, color: on ? 'var(--teal-d)' : 'var(--ink)' }}>{value}</span>
                </button>
              )
            })}
          </div>
          <div className="row" style={{ justifyContent: 'space-between', marginTop: 8 }}>
            <span className="muted" style={{ fontSize: 12 }}>{question.scaleMinLabel}</span>
            <span className="muted" style={{ fontSize: 12 }}>{question.scaleMaxLabel}</span>
          </div>
        </div>
      )}

      {question.answerType !== 'scale' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {isMulti && <div className="muted" style={{ fontSize: 13 }}>Choose up to {question.maxSelections}.</div>}
          {question.options.map((option) => {
            const selected = isMulti
              ? (answer.selectedValues || []).includes(option.value)
              : (answer.selectedValues || [])[0] === option.value
            return (
              <button key={option.value} className="card row"
                onClick={() => (isMulti ? toggleMulti(option.value) : pickSingle(option.value))}
                style={{ padding: '15px 18px', gap: 12, textAlign: 'left', borderColor: selected ? 'var(--teal)' : 'var(--line)', background: selected ? 'var(--teal-tint)' : 'var(--surface)' }}>
                {isMulti && (
                  <span style={{ width: 22, height: 22, borderRadius: 6, flexShrink: 0, border: `2px solid ${selected ? 'var(--teal)' : 'var(--line)'}`, background: selected ? 'var(--teal)' : 'transparent', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    {selected && <Check size={13} sw={3} style={{ color: '#fff' }} />}
                  </span>
                )}
                <span style={{ fontSize: 15.5, fontWeight: 500 }}>{option.label}</span>
              </button>
            )
          })}
        </div>
      )}

      {isMulti && (
        <button className="btn-primary" style={{ padding: 15, borderRadius: 14, justifyContent: 'center' }} disabled={!multiReady} onClick={advance}>
          {i < flat.length - 1 ? 'Next' : 'See my snapshot'} <ArrowRight size={17} />
        </button>
      )}

      {phase === 'submitting' && <div className="muted" style={{ textAlign: 'center' }}>Analyzing your answers…</div>}
      {submitError && <div className="inline-error" role="alert">{submitError}</div>}
    </Centered>
  )
}

function Review({ result, answers, flat, onStart }) {
  const persona = result?.analysis?.persona
  const analyzed = result?.analysisStatus === 'analyzed' && persona

  // Fallback confidence from the Q2 answer when AI analysis isn't available.
  const q2 = flat.find(({ question }) => question.code === 'Q2')?.question
  const fallbackConfidence = (q2 && answers[q2.id]?.scaleValue) || 3

  const feedback = persona?.user_facing_feedback || {}
  const strengths = Array.isArray(persona?.strengths) ? persona.strengths : []
  const gaps = Array.isArray(persona?.priority_gaps) ? persona.priority_gaps : []
  const path = persona?.first_learning_path || {}

  return (
    <Centered wide>
      <div className="row" style={{ gap: 8, color: 'var(--clay)', justifyContent: 'center' }}>
        <Sparkle size={20} /><span className="label" style={{ margin: 0 }}>Your communication snapshot</span>
      </div>
      <h1 style={{ fontSize: 27, textAlign: 'center', lineHeight: 1.3 }}>
        {analyzed ? (persona.profile_summary || 'Here’s where you’re starting') : 'Here’s where you’re starting'}
      </h1>

      {analyzed && feedback.communication_style_right_now && (
        <div className="card" style={{ padding: '20px 22px', background: 'var(--teal-tint)', borderColor: '#D9E8E3' }}>
          <div className="label" style={{ color: 'var(--teal-d)', marginBottom: 8 }}>How you communicate right now</div>
          <div style={{ fontSize: 15.5, lineHeight: 1.55 }}>{feedback.communication_style_right_now}</div>
        </div>
      )}

      {!analyzed && (
        <div className="card" style={{ padding: '20px 22px', textAlign: 'center' }}>
          <div style={{ fontWeight: 600, fontSize: 17 }}>Starting confidence: {fallbackConfidence} / 5</div>
          <div className="muted" style={{ fontSize: 14, marginTop: 4, lineHeight: 1.5 }}>
            Your answers are saved. A deeper AI snapshot appears here once analysis is enabled — for now, let’s start practicing.
          </div>
        </div>
      )}

      {analyzed && (strengths.length > 0 || gaps.length > 0) && (
        <div className="row" style={{ gap: 16, alignItems: 'stretch', flexWrap: 'wrap' }}>
          {strengths.length > 0 && (
            <div className="card" style={{ flex: 1, minWidth: 240, padding: '18px 20px', background: 'var(--sage-tint)', borderColor: '#D5E5DA', textAlign: 'left' }}>
              <div className="label" style={{ color: 'var(--sage)', marginBottom: 12 }}>Strengths</div>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                {strengths.slice(0, 3).map((item, index) => (
                  <div className="item" key={index}>
                    <span className="dot" style={{ background: 'var(--sage)' }}><Check size={12} sw={3} style={{ color: '#fff' }} /></span>
                    <span>{item.evidence || item.dimension}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
          {gaps.length > 0 && (
            <div className="card" style={{ flex: 1, minWidth: 240, padding: '18px 20px', background: 'var(--amber-tint)', borderColor: '#EAD9AE', textAlign: 'left' }}>
              <div className="label" style={{ color: 'var(--amber)', marginBottom: 12 }}>Where to grow</div>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                {gaps.slice(0, 3).map((item, index) => (
                  <div className="item" key={index}>
                    <span className="dot" style={{ background: 'var(--amber)' }}><Plus size={12} style={{ color: '#fff' }} /></span>
                    <span>{item.practice_need || item.evidence || item.dimension}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {analyzed && (path.scenario_type || feedback.first_practice_recommendation) && (
        <div className="card" style={{ padding: '18px 22px', background: 'var(--clay-tint)', borderColor: '#F0DAC8', textAlign: 'left' }}>
          <div className="label" style={{ color: 'var(--clay)', marginBottom: 8 }}>Your first practice</div>
          {path.priority_skill && (
            <div style={{ fontWeight: 600, fontSize: 18, fontFamily: "'Bricolage Grotesque'" }}>{path.priority_skill}</div>
          )}
          <div style={{ fontSize: 14.5, color: '#5f564a', marginTop: 6, lineHeight: 1.5 }}>
            {feedback.first_practice_recommendation || path.reason}
          </div>
          {path.difficulty && (
            <span className="badge" style={{ background: '#fff', color: 'var(--clay)', marginTop: 12, display: 'inline-block', textTransform: 'capitalize' }}>{path.difficulty}</span>
          )}
        </div>
      )}

      <button className="btn-primary" style={{ padding: 16, borderRadius: 15, fontSize: 16, justifyContent: 'center' }} onClick={onStart}>
        Start practicing <ArrowRight size={18} />
      </button>
    </Centered>
  )
}

function Centered({ children, wide }) {
  return (
    <div style={{ minHeight: '100dvh', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '40px 24px', background: 'var(--bg)' }}>
      <div style={{ width: '100%', maxWidth: wide ? 640 : 600, display: 'flex', flexDirection: 'column', gap: 22 }}>
        {children}
      </div>
    </div>
  )
}
