import { Sparkle, Check, Plus, ArrowRight } from '../components/Icons.jsx'
import { resourcesFor, skillLabels } from '../learning.js'

export default function Debrief({ actor, scenario, evaluation, onChallenge, onQuiz, onAgain, onHome }) {
  const evalr = evaluation || { score: 3, strengths: [], toTry: [], turns: 0, hints: 0, weakestSkill: 'engagement', dimensions: {} }
  const culture = scenario?.culture
  const focus = evalr.weakestSkill || 'engagement'
  const resources = resourcesFor(focus)
  const offset = Math.round(113 * (1 - evalr.score / 5))

  return (
    <div className="center-wrap"><div className="center-col" style={{ maxWidth: 800 }}>
      <div className="row" style={{ justifyContent: 'space-between', alignItems: 'flex-end' }}>
        <div>
          <div className="row" style={{ gap: 8, color: 'var(--clay)' }}><Sparkle size={20} /><span className="label">SESSION COMPLETE</span></div>
          <h1 style={{ fontSize: 30, marginTop: 8 }}>Let's turn feedback into progress</h1>
          <div className="muted" style={{ fontSize: 14, marginTop: 6 }}>{evalr.turns} replies · {evalr.hints} hints used</div>
        </div>
        <div className="card row" style={{ padding: '11px 16px', gap: 11, background: 'var(--teal-tint)' }}>
          <svg width="40" height="40" viewBox="0 0 44 44"><circle cx="22" cy="22" r="18" fill="none" stroke="#C4DDD6" strokeWidth="5"/><circle cx="22" cy="22" r="18" fill="none" stroke="#2F8E80" strokeWidth="5" strokeLinecap="round" strokeDasharray="113" strokeDashoffset={offset} transform="rotate(-90 22 22)"/><text x="22" y="26" textAnchor="middle" fontSize="13" fontWeight="700" fill="#256E63">{evalr.score}</text></svg>
          <div><b>{evalr.score} / 5</b><div className="muted" style={{fontSize:12}}>This session</div></div>
        </div>
      </div>

      <div className="row" style={{ gap: 16, alignItems: 'stretch', flexWrap: 'wrap' }}>
        <Feedback title="What you did well" items={evalr.strengths} color="var(--sage)" bg="var(--sage-tint)" Icon={Check} />
        <Feedback title="Try next time" items={evalr.toTry} color="var(--amber)" bg="var(--amber-tint)" Icon={Plus} />
      </div>

      <div className="card" style={{ padding: 20, background: 'var(--amber-tint)', borderColor: '#EAD9AE' }}>
        <div className="label" style={{ color: 'var(--amber)', marginBottom: 7 }}>YOUR NEXT FOCUS</div>
        <h2 style={{ fontSize: 22 }}>{skillLabels[focus]}</h2>
        <div className="row" style={{ gap: 8, marginTop: 12, flexWrap: 'wrap' }}>
          {Object.entries(evalr.dimensions || {}).map(([key, value]) => (
            <span key={key} className="badge" style={{ background: key === focus ? '#fff' : 'rgba(255,255,255,.5)', color: key === focus ? 'var(--amber)' : 'var(--ink-soft)' }}>{skillLabels[key]} {value}/5</span>
          ))}
        </div>
      </div>

      <div>
        <div className="label" style={{ marginBottom: 10 }}>Learn this skill</div>
        <div className="grid2">
          {resources.map((resource) => (
            <a key={resource.url} href={resource.url} target="_blank" rel="noreferrer" className="card" style={{ padding: 18, textDecoration: 'none', color: 'inherit' }}>
              <div className="label" style={{ color: 'var(--clay)', marginBottom: 7 }}>VIDEO · {resource.source}</div>
              <div style={{ fontWeight: 650, fontSize: 16 }}>{resource.title}</div>
              <div className="muted" style={{ fontSize: 13, lineHeight: 1.45, marginTop: 6 }}>{resource.why}</div>
              <div style={{ color: 'var(--teal-d)', fontWeight: 600, fontSize: 13, marginTop: 10 }}>Watch video !—</div>
            </a>
          ))}
        </div>
      </div>

      {culture && <div className="card" style={{ padding: 18 }}>
        <div className="label" style={{ marginBottom: 8 }}>Culture gap</div>
        <div className="gap"><div style={{ paddingRight: 18 }}><div className="muted" style={{fontSize:11}}>COMMON INSTINCT</div><div>"{culture.said}"</div></div><div style={{ paddingLeft: 18 }}><div style={{fontSize:11,color:'var(--teal-d)'}}>MORE NATURAL IN THE U.S.</div><div>"{culture.natural}"</div></div></div>
        <div className="muted" style={{ fontSize: 13, marginTop: 12 }}>{culture.why}</div>
      </div>}

      <div className="card row" style={{ padding: 18, background: 'var(--clay-tint)', flexWrap: 'wrap', gap: 14 }}>
        <div style={{ flex: 1, minWidth: 220 }}><b>Check what you learned</b><div className="muted" style={{fontSize:13,marginTop:3}}>A 3-question quiz focused on {skillLabels[focus].toLowerCase()}.</div></div>
        <button className="btn-primary" onClick={onQuiz}>Take focused quiz <ArrowRight size={17}/></button>
      </div>

      <div className="row" style={{ gap: 10, flexWrap: 'wrap' }}>
        <button className="btn-ghost" style={{flex:1}} onClick={onChallenge}>Real-world challenge</button>
        <button className="btn-ghost" onClick={onAgain}>Practice again</button>
        <button className="btn-ghost" onClick={onHome}>Home</button>
      </div>
    </div></div>
  )
}

function Feedback({ title, items, color, bg, Icon }) {
  return <div className="card" style={{ flex: 1, minWidth: 270, padding: 19, background: bg }}>
    <div className="label" style={{ color, marginBottom: 12 }}>{title}</div>
    {items.map((item, i) => <div className="item" key={i} style={{marginTop:i?10:0}}><span className="dot" style={{background:color}}><Icon size={12}/></span>{item}</div>)}
  </div>
}
