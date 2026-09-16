import { useState } from 'react'
import { quizQuestions } from '../data.js'
import { Check, Warning, ArrowRight, Sparkle } from '../components/Icons.jsx'

const feelLabels = ['Not yet', 'A little', 'Okay', 'Pretty comfortable', 'Confident']

export default function Quiz({ profile, onDone }) {
  const [step, setStep] = useState(0)          // 0..2 = MCQ, 3 = self-rating
  const [picked, setPicked] = useState(null)   // selected option index for current MCQ
  const [correct, setCorrect] = useState(0)
  const [result, setResult] = useState(null)

  const total = quizQuestions.length

  function pickMcq(idx) {
    if (picked !== null) return
    setPicked(idx)
    if (quizQuestions[step].options[idx].correct) setCorrect((c) => c + 1)
  }
  function nextMcq() {
    setPicked(null)
    setStep((s) => s + 1)
  }
  function pickFeeling(v) {
    const newConfidence = v
    setResult({ newConfidence, correct })
  }

  if (result) {
    const old = profile?.confidence ?? 3
    const delta = result.newConfidence - old
    const up = delta > 0
    return (
      <div className="center-wrap">
        <div className="center-col" style={{ maxWidth: 520, textAlign: 'center' }}>
          <div className="row" style={{ gap: 8, color: 'var(--clay)', justifyContent: 'center' }}>
            <Sparkle size={20} /><span style={{ fontSize: 12.5, fontWeight: 600, letterSpacing: '.03em' }}>YOUR RE-CHECK</span>
          </div>
          <h1 style={{ fontSize: 28 }}>{up ? "You're getting more comfortable" : "Every rep counts"}</h1>

          <div className="card" style={{ padding: '22px 24px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 18 }}>
            <div style={{ textAlign: 'center' }}>
              <div className="muted" style={{ fontSize: 12, fontWeight: 600 }}>CHECK-IN</div>
              <div style={{ fontFamily: "'Bricolage Grotesque'", fontWeight: 700, fontSize: 34 }}>{old}</div>
            </div>
            <ArrowRight size={22} />
            <div style={{ textAlign: 'center' }}>
              <div className="muted" style={{ fontSize: 12, fontWeight: 600 }}>NOW</div>
              <div style={{ fontFamily: "'Bricolage Grotesque'", fontWeight: 700, fontSize: 34, color: up ? 'var(--sage)' : 'var(--ink)' }}>{result.newConfidence}</div>
            </div>
          </div>

          <div className="card" style={{ padding: '16px 20px', background: 'var(--teal-tint)', borderColor: '#D9E8E3' }}>
            <div style={{ fontWeight: 600, fontSize: 16 }}>Quiz: {result.correct} / {total} correct</div>
            <div className="muted" style={{ fontSize: 13.5, marginTop: 3, lineHeight: 1.5 }}>
              {up
                ? "Your confidence is trending up. Keep practicing a little each day — it adds up."
                : "That's okay — comfort grows with reps. Try another conversation whenever you're ready."}
            </div>
          </div>

          <button className="btn-primary" style={{ padding: 16, borderRadius: 15, fontSize: 16, justifyContent: 'center' }}
            onClick={() => onDone(result.newConfidence, result.correct)}>
            Back to home <ArrowRight size={18} />
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="center-wrap">
      <div className="center-col" style={{ maxWidth: 560 }}>
        {/* progress */}
        <div className="row" style={{ gap: 6, justifyContent: 'center' }}>
          {[0, 1, 2, 3].map((k) => (
            <span key={k} style={{ height: 6, borderRadius: 999, width: k === step ? 26 : 12, background: k <= step ? 'var(--teal)' : 'var(--line)', transition: 'all .2s' }} />
          ))}
        </div>

        {step < total ? (
          <>
            <div className="label" style={{ textAlign: 'center' }}>Question {step + 1} of {total}</div>
            <h1 style={{ fontSize: 24, textAlign: 'center', lineHeight: 1.35 }}>{quizQuestions[step].q}</h1>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {quizQuestions[step].options.map((o, idx) => {
                const chosen = picked === idx
                const reveal = picked !== null
                let border = 'var(--line)', bg = 'var(--surface)'
                if (reveal && o.correct) { border = 'var(--sage)'; bg = 'var(--sage-tint)' }
                else if (reveal && chosen && !o.correct) { border = 'var(--clay)'; bg = '#F7E4DA' }
                return (
                  <button key={idx} className="card row" onClick={() => pickMcq(idx)}
                    style={{ padding: '15px 18px', gap: 12, textAlign: 'left', borderColor: border, background: bg, cursor: reveal ? 'default' : 'pointer' }}>
                    <span style={{ flexGrow: 1, fontSize: 15, fontWeight: 500 }}>{o.text}</span>
                    {reveal && o.correct && <span style={{ color: 'var(--sage)' }}><Check size={18} sw={2.6} /></span>}
                    {reveal && chosen && !o.correct && <span style={{ color: 'var(--clay)' }}><Warning size={18} /></span>}
                  </button>
                )
              })}
            </div>
            {picked !== null && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                <div className="card" style={{ padding: '14px 16px', fontSize: 13.5, lineHeight: 1.5, color: '#6b5f50' }}>
                  {quizQuestions[step].why}
                </div>
                <button className="btn-primary" style={{ padding: 15, borderRadius: 14, justifyContent: 'center' }} onClick={nextMcq}>
                  {step === total - 1 ? 'Last question' : 'Next question'} <ArrowRight size={17} />
                </button>
              </div>
            )}
          </>
        ) : (
          <>
            <h1 style={{ fontSize: 26, textAlign: 'center', lineHeight: 1.3 }}>Overall, how comfortable do you feel now?</h1>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {feelLabels.map((lbl, k) => (
                <button key={k} className="card row" onClick={() => pickFeeling(k + 1)}
                  style={{ padding: '15px 18px', gap: 12, textAlign: 'left' }}>
                  <span style={{ width: 30, height: 30, borderRadius: 9, background: 'var(--teal-tint)', color: 'var(--teal-d)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontFamily: "'Bricolage Grotesque'", fontWeight: 700, flexShrink: 0 }}>{k + 1}</span>
                  <span style={{ fontSize: 15, fontWeight: 500 }}>{lbl}</span>
                </button>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  )
}
