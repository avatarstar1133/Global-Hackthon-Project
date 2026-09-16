import { useState } from 'react'
import { actors, actorOrder } from '../data.js'
import { Flame, ArrowRight } from '../components/Icons.jsx'

export default function Home({ profile, onStart, onRetake }) {
  const [sel, setSel] = useState(profile?.recommended || 'friends')
  const actor = actors[sel]
  const conf = profile?.confidence ?? 3
  const offset = Math.round(113 * (1 - conf / 5))
  const rec = actors[profile?.recommended || 'friends']

  return (
    <div className="pad">
      <div className="hdr">
        <div>
          <div className="muted" style={{ fontSize: 14 }}>Good afternoon,</div>
          <h1 style={{ fontSize: 30, marginTop: 2 }}>Ready for a little practice, Minh?</h1>
        </div>
        <div className="card row" style={{ padding: '12px 18px', gap: 14, background: 'var(--teal-tint)', borderColor: '#D9E8E3' }}>
          <svg width="42" height="42" viewBox="0 0 44 44" fill="none">
            <circle cx="22" cy="22" r="18" stroke="#C4DDD6" strokeWidth="5" />
            <circle cx="22" cy="22" r="18" stroke="#2F8E80" strokeWidth="5" strokeLinecap="round" strokeDasharray="113" strokeDashoffset={offset} transform="rotate(-90 22 22)" />
          </svg>
          <div>
            <div style={{ fontWeight: 600, fontSize: 15 }}>Confidence {conf} / 5</div>
            <div className="row" style={{ gap: 5, color: 'var(--clay)', fontWeight: 600, fontSize: 13, marginTop: 2 }}>
              <Flame size={15} />5-day streak
            </div>
          </div>
        </div>
      </div>

      {/* Check-in summary */}
      <div className="card row" style={{ padding: '14px 18px', gap: 14, marginBottom: 24, background: rec.tint, borderColor: rec.border, flexWrap: 'wrap' }}>
        <span style={{ width: 40, height: 40, borderRadius: 12, background: rec.color, color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontFamily: "'Bricolage Grotesque'", fontWeight: 600, flexShrink: 0 }}>{rec.persona.initial}</span>
        <div style={{ flexGrow: 1, minWidth: 200 }}>
          <div className="label" style={{ color: rec.color, marginBottom: 3 }}>Based on your check-in</div>
          <div style={{ fontSize: 15, fontWeight: 500 }}>We suggest focusing on <b>{rec.label.toLowerCase()}</b> — {rec.tagline.toLowerCase()}.</div>
        </div>
        <button className="btn-ghost" style={{ padding: '9px 16px', borderRadius: 999, fontSize: 13.5 }} onClick={onRetake}>Retake check-in</button>
      </div>

      {/* Actor picker */}
      <h3 style={{ fontSize: 13, textTransform: 'uppercase', letterSpacing: '.05em', color: 'var(--ink-soft)', marginBottom: 14 }}>Who do you want to practice with?</h3>
      <div className="grid3" style={{ display: 'grid', gridTemplateColumns: 'repeat(3,minmax(0,1fr))', gap: 14, marginBottom: 28 }}>
        {actorOrder.map((key) => {
          const a = actors[key]
          const on = key === sel
          return (
            <button key={key} className="card" onClick={() => setSel(key)}
              style={{ padding: '18px 18px', textAlign: 'left', borderColor: on ? a.color : 'var(--line)', background: on ? a.tint : 'var(--surface)', transition: 'all .12s' }}>
              <div className="row" style={{ gap: 12, marginBottom: 10 }}>
                <span style={{ width: 42, height: 42, borderRadius: 12, background: a.color, color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center', fontFamily: "'Bricolage Grotesque'", fontWeight: 600, flexShrink: 0 }}>{a.persona.initial}</span>
                <div>
                  <div style={{ fontWeight: 600, fontSize: 16 }}>{a.label}</div>
                  <div className="muted" style={{ fontSize: 12.5 }}>{a.tagline}</div>
                </div>
              </div>
              <div className="muted" style={{ fontSize: 12.5, lineHeight: 1.45 }}>{a.about}</div>
            </button>
          )
        })}
      </div>

      {/* Scenarios for the selected actor */}
      <div className="row" style={{ gap: 10, marginBottom: 14 }}>
        <h3 style={{ fontSize: 13, textTransform: 'uppercase', letterSpacing: '.05em', color: 'var(--ink-soft)' }}>Practice with {actor.persona.name}</h3>
        <span className="badge" style={{ background: actor.tint, color: actor.color }}>{actor.label}</span>
      </div>
      <div className="grid2">
        {actor.scenarios.map((s) => (
          <button key={s.id} className="card scn" onClick={() => onStart(actor.key, s)}>
            <span className="ic" style={{ background: actor.tint, color: actor.color }}>
              <ArrowRight size={22} />
            </span>
            <span style={{ flexGrow: 1 }}>
              <span style={{ display: 'block', fontWeight: 600, fontSize: 16 }}>{s.title}</span>
              <span className="muted" style={{ fontSize: 13.5 }}>{s.desc}</span>
            </span>
            <span className="badge" style={{ background: s.level === 'Easy' ? 'var(--teal-tint)' : s.level === 'Medium' ? 'var(--amber-tint)' : '#F7E4DA', color: s.level === 'Easy' ? 'var(--sage)' : s.level === 'Medium' ? 'var(--amber)' : 'var(--clay)' }}>{s.level}</span>
          </button>
        ))}
      </div>
    </div>
  )
}
