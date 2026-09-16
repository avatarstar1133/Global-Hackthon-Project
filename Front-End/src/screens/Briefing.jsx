import { ChevronLeft, Target, ArrowRight } from '../components/Icons.jsx'

export default function Briefing({ actor, scenario, onBack, onStart }) {
  const p = actor.persona
  return (
    <div className="center-wrap">
      <div className="center-col">
        <button className="back" onClick={onBack}><ChevronLeft size={18} />Back to home</button>
        <div className="row" style={{ gap: 10 }}>
          <span className="badge" style={{ background: actor.tint, color: actor.color }}>{actor.label}</span>
          <span className="badge" style={{ background: 'var(--surface)', border: '1px solid var(--line)', color: 'var(--ink-soft)' }}>{scenario.level}</span>
        </div>
        <h1 style={{ fontSize: 32 }}>{scenario.title}</h1>

        <div>
          <div className="label" style={{ marginBottom: 9 }}>The scene</div>
          <div className="card" style={{ padding: '18px 20px', fontSize: 15.5, lineHeight: 1.55, color: '#514a41' }}>
            {scenario.scene}
          </div>
        </div>

        <div className="row" style={{ gap: 20, alignItems: 'stretch' }}>
          <div style={{ flex: 1 }}>
            <div className="label" style={{ marginBottom: 9 }}>You'll be talking to</div>
            <div className="card row" style={{ padding: '16px 18px', gap: 14, height: 'calc(100% - 29px)' }}>
              <div style={{ width: 50, height: 50, borderRadius: '50%', background: p.accent, color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontFamily: "'Bricolage Grotesque'", fontWeight: 600, fontSize: 20, flexShrink: 0 }}>{p.initial}</div>
              <div><div style={{ fontWeight: 600, fontSize: 17 }}>{p.name}</div><div className="muted" style={{ fontSize: 13.5 }}>{p.role}</div></div>
            </div>
          </div>
          <div style={{ flex: 1 }}>
            <div className="label" style={{ marginBottom: 9 }}>Your goal</div>
            <div className="card row" style={{ padding: '16px 18px', gap: 13, background: 'var(--clay-tint)', borderColor: '#F0DAC8', height: 'calc(100% - 29px)' }}>
              <span style={{ color: 'var(--clay)', flexShrink: 0 }}><Target size={24} /></span>
              <div style={{ fontSize: 15, fontWeight: 500 }}>{scenario.goal}</div>
            </div>
          </div>
        </div>

        <div>
          <div className="row" style={{ justifyContent: 'space-between', marginBottom: 11 }}>
            <div className="label">How direct should {p.name} be?</div>
            <div style={{ fontSize: 13, fontWeight: 600, color: 'var(--teal-d)' }}>Balanced</div>
          </div>
          <div style={{ position: 'relative', height: 8, background: 'var(--line)', borderRadius: 999 }}>
            <div style={{ position: 'absolute', left: 0, top: 0, height: 8, width: '52%', background: 'var(--teal)', borderRadius: 999 }} />
            <div style={{ position: 'absolute', left: '52%', top: '50%', transform: 'translate(-50%,-50%)', width: 22, height: 22, borderRadius: '50%', background: '#fff', border: '3px solid var(--teal)', boxShadow: '0 1px 3px rgba(0,0,0,.1)' }} />
          </div>
          <div className="row" style={{ justifyContent: 'space-between', marginTop: 9 }}>
            <span className="muted" style={{ fontSize: 12 }}>Gentle &amp; indirect</span>
            <span className="muted" style={{ fontSize: 12 }}>Direct, typical American</span>
          </div>
        </div>

        <button className="btn-primary" style={{ padding: 16, borderRadius: 15, fontSize: 16, marginTop: 4 }} onClick={onStart}>
          Start the conversation <ArrowRight size={18} />
        </button>
      </div>
    </div>
  )
}
