import { skillLabels } from '../learning.js'
import { ArrowRight, Check, Chart } from '../components/Icons.jsx'

function loadHistory() {
  try { return JSON.parse(localStorage.getItem('bridge_history') || '[]') } catch { return [] }
}

export default function Progress({ profile, onContinue }) {
  const history = loadHistory().slice().reverse()
  const latest = history[0]
  const focus = latest?.weakestSkill || profile?.focusSkill || 'engagement'
  const scores = history.slice(0, 6).reverse().map((x) => x.score)
  const improved = scores.length > 1 && scores[scores.length - 1] > scores[0]

  return (
    <div className="pad">
      <div className="hdr">
        <div>
          <div className="muted" style={{ fontSize: 14 }}>Your learning path</div>
          <h1 style={{ fontSize: 30, marginTop: 2 }}>Progress grows one cycle at a time</h1>
        </div>
      </div>

      <div className="card" style={{ padding: 22, marginBottom: 22 }}>
        <div className="label" style={{ marginBottom: 14 }}>Current cycle</div>
        <div className="row" style={{ gap: 10, flexWrap: 'wrap' }}>
          {['Practice', 'Review', 'Learn', 'Quiz', 'Practice again'].map((step, i) => (
            <div key={step} className="row" style={{ gap: 8 }}>
              <span style={{ width: 30, height: 30, borderRadius: 999, display: 'grid', placeItems: 'center', background: i < 4 && latest ? 'var(--teal)' : 'var(--line)', color: i < 4 && latest ? '#fff' : 'var(--ink-soft)' }}>
                {i < 4 && latest ? <Check size={14} /> : i + 1}
              </span>
              <span style={{ fontSize: 13.5, fontWeight: 600 }}>{step}</span>
              {i < 4 && <ArrowRight size={15} />}
            </div>
          ))}
        </div>
      </div>

      <div className="grid2" style={{ marginBottom: 22 }}>
        <div className="card" style={{ padding: 20, background: 'var(--amber-tint)', borderColor: '#EAD9AE' }}>
          <div className="label" style={{ color: 'var(--amber)', marginBottom: 8 }}>Current focus</div>
          <h2 style={{ fontSize: 21 }}>{skillLabels[focus]}</h2>
          <p className="muted" style={{ fontSize: 14, lineHeight: 1.5, marginTop: 6 }}>Your next practice should test this skill again in context.</p>
          <button className="btn-primary" style={{ marginTop: 14 }} onClick={onContinue}>Continue learning <ArrowRight size={17} /></button>
        </div>
        <div className="card" style={{ padding: 20 }}>
          <div className="label" style={{ marginBottom: 8 }}>Evidence</div>
          <div style={{ fontFamily: "'Bricolage Grotesque'", fontWeight: 700, fontSize: 30 }}>{history.length}</div>
          <div className="muted" style={{ fontSize: 13.5 }}>completed practice sessions</div>
          <div style={{ marginTop: 13, color: improved ? 'var(--sage)' : 'var(--ink-soft)', fontWeight: 600, fontSize: 14 }}>
            {improved ? 'Your recent session scores are trending up.' : 'Complete another practice cycle to compare behavior.'}
          </div>
        </div>
      </div>

      <div className="label" style={{ marginBottom: 10 }}>Recent sessions</div>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
        {!history.length && <div className="card muted" style={{ padding: 20 }}>No completed sessions yet.</div>}
        {history.slice(0, 6).map((item) => (
          <div className="card row" key={item.id} style={{ padding: '14px 18px', gap: 14 }}>
            <span className="ic" style={{ background: 'var(--teal-tint)', color: 'var(--teal-d)' }}><Chart size={19} /></span>
            <div style={{ flex: 1 }}>
              <div style={{ fontWeight: 600 }}>{item.scenario}</div>
              <div className="muted" style={{ fontSize: 12.5 }}>{item.actor} - Focus: {skillLabels[item.weakestSkill] || 'Conversation practice'}</div>
            </div>
            <span className="badge" style={{ background: 'var(--teal-tint)', color: 'var(--teal-d)' }}>{item.score}/5</span>
          </div>
        ))}
      </div>
    </div>
  )
}
