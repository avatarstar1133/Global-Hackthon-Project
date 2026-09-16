import { useState } from 'react'
import { ChevronLeft, Target, ArrowRight } from '../components/Icons.jsx'

const directnessLabels = ['Very gentle', 'Gentle', 'Balanced', 'Direct', 'Very direct']

export default function Briefing({ actor, scenario, onBack, onStart }) {
  const [directness, setDirectness] = useState(3)
  const persona = actor.persona

  return (
    <div className="center-wrap">
      <div className="center-col briefing-col">
        <button className="back" onClick={onBack}><ChevronLeft size={18} />Back to home</button>
        <div className="row" style={{ gap: 10 }}>
          <span className="badge" style={{ background: actor.tint, color: actor.color }}>{actor.label}</span>
          <span className="badge neutral-badge">{scenario.level}</span>
        </div>
        <h1>{scenario.title}</h1>

        <div>
          <div className="label block-label">The scene</div>
          <div className="card scene-card">{scenario.scene}</div>
        </div>

        <div className="briefing-grid">
          <div>
            <div className="label block-label">You will be talking to</div>
            <div className="card person-card">
              <div className="large-avatar" style={{ background: persona.accent }}>{persona.initial}</div>
              <div><div className="person-name">{persona.name}</div><div className="muted">{persona.role}</div></div>
            </div>
          </div>
          <div>
            <div className="label block-label">Your goal</div>
            <div className="card goal-card"><Target size={24} /><div>{scenario.goal}</div></div>
          </div>
        </div>

        <div className="directness-control">
          <div className="row directness-heading">
            <div className="label">How direct should {persona.name} be?</div>
            <div className="directness-value">{directnessLabels[directness - 1]}</div>
          </div>
          <input type="range" min="1" max="5" value={directness} onChange={(event) => setDirectness(Number(event.target.value))} aria-label={`Directness level ${directness}`} />
          <div className="row directness-scale"><span>Gentle and indirect</span><span>Typical American directness</span></div>
        </div>

        <button className="btn-primary start-button" onClick={() => onStart(directness)}>
          Start the conversation <ArrowRight size={18} />
        </button>
      </div>
    </div>
  )
}