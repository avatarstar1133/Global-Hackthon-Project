import { useEffect, useState } from 'react'
import { Flame, ArrowRight } from '../components/Icons.jsx'

export default function Home({ profile, catalog, onStart, onRetake }) {
  const actorKeys = Object.keys(catalog || {})
  const preferred = catalog?.[profile?.recommended] ? profile.recommended : actorKeys[0]
  const [selectedKey, setSelectedKey] = useState(preferred)

  useEffect(() => {
    if (!catalog?.[selectedKey]) setSelectedKey(preferred)
  }, [catalog, preferred, selectedKey])

  if (!catalog || !selectedKey) return <div className="pad"><div className="empty-state">No practice actors are available.</div></div>

  const actor = catalog[selectedKey]
  const confidence = profile?.confidence ?? 3
  const offset = Math.round(113 * (1 - confidence / 5))
  const recommended = catalog[profile?.recommended] || actor

  return (
    <div className="pad home-page">
      <header className="hdr">
        <div>
          <div className="eyebrow">Communication practice</div>
          <h1>Ready for a little practice?</h1>
          <p className="page-intro">Choose a situation, try the conversation, then turn feedback into one focused next step.</p>
        </div>
        <div className="confidence-card">
          <svg width="48" height="48" viewBox="0 0 44 44" fill="none" aria-hidden="true">
            <circle cx="22" cy="22" r="18" stroke="#C4DDD6" strokeWidth="5" />
            <circle cx="22" cy="22" r="18" stroke="#2F8E80" strokeWidth="5" strokeLinecap="round" strokeDasharray="113" strokeDashoffset={offset} transform="rotate(-90 22 22)" />
          </svg>
          <div>
            <div className="confidence-value">Confidence {confidence} / 5</div>
            <div className="streak"><Flame size={15} />Keep the cycle going</div>
          </div>
        </div>
      </header>

      <section className="recommendation" style={{ '--actor-color': recommended.color, '--actor-tint': recommended.tint, '--actor-border': recommended.border }}>
        <span className="persona-tile">{recommended.persona.initial}</span>
        <div>
          <div className="label">Based on your check-in</div>
          <div className="recommendation-copy">Start with <b>{recommended.label.toLowerCase()}</b> — {recommended.tagline.toLowerCase()}.</div>
        </div>
        <button className="btn-ghost compact-button" onClick={onRetake}>Retake check-in</button>
      </section>

      <section>
        <div className="section-heading">
          <div>
            <div className="label">Choose your conversation partner</div>
            <h2>Practice with an American peer or professor</h2>
          </div>
        </div>
        <div className="actor-grid">
          {actorKeys.map((key) => {
            const item = catalog[key]
            const active = key === selectedKey
            return (
              <button key={key} className={`actor-card ${active ? 'selected' : ''}`} onClick={() => setSelectedKey(key)} style={{ '--actor-color': item.color, '--actor-tint': item.tint, '--actor-border': item.border }}>
                <span className="persona-tile">{item.persona.initial}</span>
                <span>
                  <span className="actor-title">{item.label}</span>
                  <span className="actor-tagline">{item.tagline}</span>
                  <span className="actor-about">{item.about}</span>
                </span>
              </button>
            )
          })}
        </div>
      </section>

      <section>
        <div className="section-heading scenario-heading">
          <div>
            <div className="label">Practice with {actor.persona.name}</div>
            <h2>Pick a situation to rehearse</h2>
          </div>
          <span className="badge" style={{ background: actor.tint, color: actor.color }}>{actor.label}</span>
        </div>
        <div className="scenario-grid">
          {actor.scenarios.map((scenario) => (
            <button key={scenario.id} className="card scn" onClick={() => onStart(actor.key, scenario)}>
              <span className="ic" style={{ background: actor.tint, color: actor.color }}><ArrowRight size={22} /></span>
              <span className="scenario-copy">
                <span className="scenario-title">{scenario.title}</span>
                <span className="muted">{scenario.desc}</span>
              </span>
              <span className={`badge difficulty ${scenario.level.toLowerCase()}`}>{scenario.level}</span>
            </button>
          ))}
        </div>
      </section>
    </div>
  )
}