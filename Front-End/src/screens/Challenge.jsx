import { useState } from 'react'
import { ChevronLeft, Shield, Check } from '../components/Icons.jsx'

const log = [
  { text: 'Said hi to someone new in the dining hall', when: '3 days ago' },
  { text: 'Sat with a group of classmates at lunch', when: 'last week' },
]

export default function Challenge({ onBack }) {
  const [done, setDone] = useState(false)

  return (
    <div className="center-wrap">
      <div className="center-col">
        <button className="back" onClick={onBack}><ChevronLeft size={18} />Back</button>

        <div className="card" style={{ padding: '28px 30px', background: 'var(--clay-tint)', borderColor: '#F0DAC8' }}>
          <div className="row" style={{ gap: 18 }}>
            <div style={{ width: 60, height: 60, borderRadius: 17, background: 'var(--clay)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#fff', flexShrink: 0 }}>
              <Shield size={30} />
            </div>
            <div>
              <div className="label" style={{ color: 'var(--clay)', marginBottom: 6 }}>This week's challenge</div>
              <h1 style={{ fontSize: 25 }}>Ask one question in class</h1>
            </div>
          </div>
          <div style={{ fontSize: 15, lineHeight: 1.55, color: '#6b5a44', marginTop: 16 }}>
            In your next lecture, raise your hand and ask the professor or a classmate <b>any one question</b>. It can be small — what matters is that you speak up. You've already rehearsed the feeling here.
          </div>
        </div>

        <div className="row" style={{ gap: 16, alignItems: 'stretch', flexWrap: 'wrap' }}>
          <button className="card row" style={{ padding: '18px 22px', gap: 13, alignSelf: 'flex-start', background: done ? 'var(--sage-tint)' : '' }} onClick={() => setDone((d) => !d)}>
            <span className={done ? 'check done' : 'check'}><Check size={15} sw={3} style={{ color: '#fff' }} /></span>
            <span style={{ fontWeight: 600, fontSize: 16 }}>{done ? 'Done — nice work!' : 'I did it!'}</span>
          </button>
          <div style={{ flexGrow: 1, minWidth: 280 }}>
            <div className="label" style={{ marginBottom: 8 }}>How did it feel? <span style={{ textTransform: 'none', fontWeight: 400 }}>(optional)</span></div>
            <textarea className="field" style={{ width: '100%', minHeight: 70, resize: 'vertical', padding: '14px 16px', border: '1px solid var(--line)', borderRadius: 14, background: 'var(--surface)', fontSize: 14, lineHeight: 1.5 }} placeholder="Write a few lines about the moment you spoke up…" />
          </div>
        </div>

        <div>
          <div className="label" style={{ marginBottom: 11 }}>Challenge log</div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
            {log.map((l, i) => (
              <div className="card row" key={i} style={{ padding: '14px 18px', gap: 13, background: 'var(--sage-tint)', borderColor: '#D5E5DA' }}>
                <div style={{ width: 24, height: 24, borderRadius: '50%', background: 'var(--sage)', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
                  <Check size={13} sw={3} style={{ color: '#fff' }} />
                </div>
                <div style={{ flexGrow: 1, fontSize: 14.5, fontWeight: 500 }}>{l.text}</div>
                <div className="muted" style={{ fontSize: 13 }}>{l.when}</div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}
