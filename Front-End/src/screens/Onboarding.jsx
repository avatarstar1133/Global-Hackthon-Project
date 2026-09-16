import { useState } from 'react'
import { actors, actorOrder, originQuestion, timeQuestion, scaleQuestions, scaleLabels } from '../data.js'
import { Bridge, ArrowRight, ChevronLeft, Sparkle } from '../components/Icons.jsx'

// step ids in order
const steps = ['origin', 'time', 'class', 'disagree', 'smalltalk', 'hardest']

export default function Onboarding({ onDone }) {
  const [i, setI] = useState(0)
  const [ans, setAns] = useState({})
  const [showResult, setShowResult] = useState(false)

  const set = (id, val) => {
    const next = { ...ans, [id]: val }
    setAns(next)
    setTimeout(() => {
      if (i < steps.length - 1) setI(i + 1)
      else setShowResult(true)
    }, 160)
  }

  if (showResult) {
    const scales = [ans.class, ans.disagree, ans.smalltalk]
    const avg = scales.reduce((a, b) => a + b, 0) / scales.length
    const confidence = Math.max(1, Math.round((avg / 4) * 5))
    const rec = ans.hardest || 'friends'
    const profile = { origin: ans.origin, time: ans.time, confidence, recommended: rec, ratings: ans }
    return <Result profile={profile} onDone={() => onDone(profile)} />
  }

  const step = steps[i]
  return (
    <div style={wrap}>
      <div style={{ width: '100%', maxWidth: 560, display: 'flex', flexDirection: 'column', gap: 26 }}>
        <div className="brand" style={{ padding: 0, justifyContent: 'center' }}>
          <span className="mark"><Bridge size={19} /></span>Bridge
        </div>

        {/* progress */}
        <div className="row" style={{ gap: 6, justifyContent: 'center' }}>
          {steps.map((_, k) => (
            <span key={k} style={{ height: 6, borderRadius: 999, width: k === i ? 26 : 12, background: k <= i ? 'var(--teal)' : 'var(--line)', transition: 'all .2s' }} />
          ))}
        </div>

        {i > 0 && (
          <button className="back" onClick={() => setI(i - 1)} style={{ alignSelf: 'flex-start' }}><ChevronLeft size={18} />Back</button>
        )}

        {step === 'origin' && <Choice q={originQuestion.q} options={originQuestion.options} value={ans.origin} onPick={(v) => set('origin', v)} />}
        {step === 'time' && <Choice q={timeQuestion.q} options={timeQuestion.options} value={ans.time} onPick={(v) => set('time', v)} />}
        {scaleQuestions.map((sq) => step === sq.id && (
          <Scale key={sq.id} q={sq.q} value={ans[sq.id]} onPick={(v) => set(sq.id, v)} />
        ))}
        {step === 'hardest' && <ActorPick value={ans.hardest} onPick={(v) => set('hardest', v)} />}
      </div>
    </div>
  )
}

function Choice({ q, options, value, onPick }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      <h1 style={{ fontSize: 27, textAlign: 'center', lineHeight: 1.3 }}>{q}</h1>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
        {options.map((o) => (
          <button key={o} className="card" onClick={() => onPick(o)}
            style={{ padding: '16px 20px', textAlign: 'left', fontSize: 16, fontWeight: 500, borderColor: value === o ? 'var(--teal)' : 'var(--line)', background: value === o ? 'var(--teal-tint)' : 'var(--surface)' }}>
            {o}
          </button>
        ))}
      </div>
    </div>
  )
}

function Scale({ q, value, onPick }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
      <h1 style={{ fontSize: 27, textAlign: 'center', lineHeight: 1.3 }}>{q}</h1>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,1fr)', gap: 10 }}>
        {scaleLabels.map((lbl, k) => {
          const v = k + 1
          const on = value === v
          return (
            <button key={v} className="card" onClick={() => onPick(v)}
              style={{ padding: '18px 10px', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 8, borderColor: on ? 'var(--teal)' : 'var(--line)', background: on ? 'var(--teal-tint)' : 'var(--surface)' }}>
              <span style={{ fontFamily: "'Bricolage Grotesque'", fontWeight: 700, fontSize: 20, color: on ? 'var(--teal-d)' : 'var(--ink)' }}>{v}</span>
              <span style={{ fontSize: 11.5, color: 'var(--ink-soft)', textAlign: 'center', lineHeight: 1.2 }}>{lbl}</span>
            </button>
          )
        })}
      </div>
    </div>
  )
}

function ActorPick({ value, onPick }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
      <h1 style={{ fontSize: 27, textAlign: 'center', lineHeight: 1.3 }}>Which conversations feel hardest right now?</h1>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
        {actorOrder.map((key) => {
          const a = actors[key]
          const on = value === key
          return (
            <button key={key} className="card row" onClick={() => onPick(key)}
              style={{ padding: '16px 18px', gap: 14, textAlign: 'left', borderColor: on ? a.color : 'var(--line)', background: on ? a.tint : 'var(--surface)' }}>
              <span style={{ width: 44, height: 44, borderRadius: 12, background: a.color, color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontFamily: "'Bricolage Grotesque'", fontWeight: 600, flexShrink: 0 }}>{a.persona.initial}</span>
              <span style={{ flexGrow: 1 }}>
                <span style={{ display: 'block', fontWeight: 600, fontSize: 16 }}>{a.label}</span>
                <span className="muted" style={{ fontSize: 13.5 }}>{a.tagline}</span>
              </span>
            </button>
          )
        })}
      </div>
    </div>
  )
}

function Result({ profile, onDone }) {
  const a = actors[profile.recommended]
  const offset = Math.round(113 * (1 - profile.confidence / 5))
  return (
    <div style={wrap}>
      <div style={{ width: '100%', maxWidth: 520, display: 'flex', flexDirection: 'column', gap: 22, textAlign: 'center' }}>
        <div className="row" style={{ gap: 8, color: 'var(--clay)', justifyContent: 'center' }}>
          <Sparkle size={20} /><span style={{ fontSize: 12.5, fontWeight: 600, letterSpacing: '.03em' }}>YOUR CHECK-IN</span>
        </div>
        <h1 style={{ fontSize: 28 }}>Here's where you're starting</h1>

        <div className="card" style={{ padding: '24px 26px', display: 'flex', alignItems: 'center', gap: 20 }}>
          <svg width="72" height="72" viewBox="0 0 44 44" fill="none" style={{ flexShrink: 0 }}>
            <circle cx="22" cy="22" r="18" stroke="#C4DDD6" strokeWidth="5" />
            <circle cx="22" cy="22" r="18" stroke="#2F8E80" strokeWidth="5" strokeLinecap="round" strokeDasharray="113" strokeDashoffset={offset} transform="rotate(-90 22 22)" />
            <text x="22" y="26" textAnchor="middle" fontSize="13" fontWeight="700" fill="#256E63" fontFamily="Bricolage Grotesque">{profile.confidence}</text>
          </svg>
          <div style={{ textAlign: 'left' }}>
            <div style={{ fontWeight: 600, fontSize: 17 }}>Starting confidence: {profile.confidence} / 5</div>
            <div className="muted" style={{ fontSize: 14, marginTop: 3, lineHeight: 1.45 }}>
              We'll adjust the difficulty as you practice. There are no wrong answers here.
            </div>
          </div>
        </div>

        <div className="card" style={{ padding: '20px 22px', background: a.tint, borderColor: a.border, textAlign: 'left' }}>
          <div className="label" style={{ color: a.color, marginBottom: 8 }}>We suggest starting with</div>
          <div style={{ fontWeight: 600, fontSize: 19, fontFamily: "'Bricolage Grotesque'" }}>{a.label} — {a.tagline}</div>
          <div style={{ fontSize: 14, color: '#5f564a', marginTop: 6, lineHeight: 1.5 }}>{a.about}</div>
        </div>

        <button className="btn-primary" style={{ padding: 16, borderRadius: 15, fontSize: 16, justifyContent: 'center' }} onClick={onDone}>
          Start practicing <ArrowRight size={18} />
        </button>
      </div>
    </div>
  )
}

const wrap = {
  minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center',
  padding: '40px 24px', background: 'var(--bg)',
}
