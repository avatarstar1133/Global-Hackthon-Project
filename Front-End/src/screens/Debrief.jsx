import { Sparkle, Check, Plus, ArrowRight } from '../components/Icons.jsx'
import { skillLabels } from '../learning.js'

export default function Debrief({ evaluation, onChallenge, onQuiz, onAgain, onHome }) {
  const result = evaluation || { score: 3, strengths: [], toTry: [], turns: 0, hints: 0, weakestSkill: 'engagement', dimensions: {} }
  const focus = result.weakestSkill || 'engagement'
  const resources = result.learningPlan?.resources || []
  const culture = result.cultureGap
  const offset = Math.round(113 * (1 - result.score / 5))

  return <div className="center-wrap"><div className="center-col debrief-col">
    <div className="debrief-header">
      <div>
        <div className="row completion-label"><Sparkle size={20} /><span className="label">Session complete</span></div>
        <h1>Turn this feedback into progress</h1>
        <div className="muted session-meta">{result.turns} replies · {result.hints} hints used</div>
      </div>
      <div className="score-card">
        <svg width="44" height="44" viewBox="0 0 44 44" aria-hidden="true"><circle cx="22" cy="22" r="18" fill="none" stroke="#C4DDD6" strokeWidth="5" /><circle cx="22" cy="22" r="18" fill="none" stroke="#2F8E80" strokeWidth="5" strokeLinecap="round" strokeDasharray="113" strokeDashoffset={offset} transform="rotate(-90 22 22)" /><text x="22" y="26" textAnchor="middle" fontSize="13" fontWeight="700" fill="#256E63">{result.score}</text></svg>
        <div><b>{result.score} / 5</b><div className="muted">This session</div></div>
      </div>
    </div>

    {result.summary && <div className="summary-callout">{result.summary}</div>}

    <div className="feedback-grid">
      <Feedback title="What you did well" items={result.strengths} color="var(--sage)" bg="var(--sage-tint)" Icon={Check} />
      <Feedback title="Try next time" items={result.toTry} color="var(--amber)" bg="var(--amber-tint)" Icon={Plus} />
    </div>

    <div className="focus-panel">
      <div className="label">Your next focus</div>
      <h2>{skillLabels[focus] || 'Conversation practice'}</h2>
      <div className="dimension-list">
        {Object.entries(result.dimensions || {}).map(([key, value]) => (
          <span key={key} className={key === focus ? 'focus-dimension' : ''}>{skillLabels[key] || key} <b>{value}/5</b></span>
        ))}
      </div>
    </div>

    <section>
      <div className="section-heading compact"><div><div className="label">Learn this skill</div><h2>Two focused resources</h2></div></div>
      <div className="resource-grid">
        {resources.map((resource) => (
          <a key={resource.url} href={resource.url} target="_blank" rel="noreferrer" className="resource-link">
            <div className="label">Video · {resource.source}</div>
            <div className="resource-title">{resource.title}</div>
            <div className="muted resource-copy">{resource.why}</div>
            <div className="resource-action">Watch video <ArrowRight size={15} /></div>
          </a>
        ))}
      </div>
    </section>

    {culture && <div className="culture-gap">
      <div className="label">Culture gap</div>
      <div className="culture-grid">
        <div><span>What you said</span><p>“{culture.original}”</p></div>
        <div><span>A clearer alternative</span><p>“{culture.alternative}”</p></div>
      </div>
      <p className="muted">{culture.explanation}</p>
    </div>}

    <div className="quiz-callout">
      <div><b>Check what you learned</b><div className="muted">A short quiz focused on {(skillLabels[focus] || 'this skill').toLowerCase()}.</div></div>
      <button className="btn-primary" onClick={onQuiz}>Take focused quiz <ArrowRight size={17} /></button>
    </div>

    <div className="debrief-actions">
      <button className="btn-ghost" onClick={onChallenge}>Real-world challenge</button>
      <button className="btn-ghost" onClick={onAgain}>Practice again</button>
      <button className="btn-ghost" onClick={onHome}>Home</button>
    </div>
  </div></div>
}

function Feedback({ title, items, color, bg, Icon }) {
  return <div className="feedback-block" style={{ '--feedback-color': color, '--feedback-bg': bg }}>
    <div className="label">{title}</div>
    {items.length === 0 && <div className="muted feedback-empty">No feedback was returned.</div>}
    {items.map((item, index) => <div className="item" key={index}><span className="dot"><Icon size={12} /></span>{item}</div>)}
  </div>
}