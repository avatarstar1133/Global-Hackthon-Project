import { Sparkle, Check, Plus, ArrowRight } from '../components/Icons.jsx'

export default function Debrief({ actor, scenario, evaluation, onChallenge, onQuiz, onAgain, onHome }) {
  const evalr = evaluation || { score: 3, strengths: [], toTry: [], turns: 0, hints: 0 }
  const culture = scenario?.culture
  const offset = Math.round(113 * (1 - evalr.score / 5))

  return (
    <div className="center-wrap">
      <div className="center-col" style={{ maxWidth: 760 }}>
        <div className="row" style={{ justifyContent: 'space-between', alignItems: 'flex-end' }}>
          <div>
            <div className="row" style={{ gap: 8, color: 'var(--clay)' }}>
              <Sparkle size={20} /><span style={{ fontSize: 12.5, fontWeight: 600, letterSpacing: '.03em' }}>SESSION COMPLETE</span>
            </div>
            <h1 style={{ fontSize: 30, marginTop: 8 }}>Let's look back together</h1>
            <div className="muted" style={{ fontSize: 14, marginTop: 6 }}>
              {evalr.turns} {evalr.turns === 1 ? 'reply' : 'replies'} · {evalr.hints} {evalr.hints === 1 ? 'hint' : 'hints'} used
            </div>
          </div>
          <div className="card row" style={{ padding: '11px 16px', gap: 11, background: 'var(--teal-tint)', borderColor: '#D9E8E3' }}>
            <svg width="40" height="40" viewBox="0 0 44 44" fill="none">
              <circle cx="22" cy="22" r="18" stroke="#C4DDD6" strokeWidth="5" />
              <circle cx="22" cy="22" r="18" stroke="#2F8E80" strokeWidth="5" strokeLinecap="round" strokeDasharray="113" strokeDashoffset={offset} transform="rotate(-90 22 22)" />
              <text x="22" y="26" textAnchor="middle" fontSize="13" fontWeight="700" fill="#256E63" fontFamily="Bricolage Grotesque">{evalr.score}</text>
            </svg>
            <div><div style={{ fontWeight: 600, fontSize: 14 }}>This session</div><div className="muted" style={{ fontSize: 12 }}>{evalr.score} / 5</div></div>
          </div>
        </div>

        <div className="row" style={{ gap: 16, alignItems: 'stretch', flexWrap: 'wrap' }}>
          <div className="card" style={{ flex: 1.4, minWidth: 280, padding: '18px 20px', background: 'var(--sage-tint)', borderColor: '#D5E5DA' }}>
            <div className="label" style={{ color: 'var(--sage)', marginBottom: 13 }}>What you did well</div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 11 }}>
              {evalr.strengths.map((s, i) => (
                <div className="item" key={i}>
                  <span className="dot" style={{ background: 'var(--sage)' }}><Check size={12} sw={3} style={{ color: '#fff' }} /></span>{s}
                </div>
              ))}
            </div>
          </div>
          <div className="card" style={{ flex: 1, minWidth: 240, padding: '18px 20px', background: 'var(--amber-tint)', borderColor: '#EAD9AE' }}>
            <div className="label" style={{ color: 'var(--amber)', marginBottom: 13 }}>Try next time</div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 11 }}>
              {evalr.toTry.map((t, i) => (
                <div className="item" key={i}>
                  <span className="dot" style={{ background: 'var(--amber)' }}><Plus size={12} style={{ color: '#fff' }} /></span>{t}
                </div>
              ))}
            </div>
          </div>
        </div>

        {culture && (
          <div>
            <div className="label" style={{ marginBottom: 9 }}>Culture gap</div>
            <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
              <div className="gap">
                <div style={{ padding: '16px 18px', borderRight: '1px solid var(--line)' }}>
                  <div className="muted" style={{ fontSize: 11, fontWeight: 600, marginBottom: 6 }}>A COMMON INSTINCT</div>
                  <div style={{ fontSize: 14.5, lineHeight: 1.45 }}>"{culture.said}"</div>
                </div>
                <div style={{ padding: '16px 18px', background: 'var(--teal-tint)' }}>
                  <div style={{ fontSize: 11, fontWeight: 600, marginBottom: 6, color: 'var(--teal-d)' }}>MORE NATURAL IN THE U.S.</div>
                  <div style={{ fontSize: 14.5, lineHeight: 1.45 }}>"{culture.natural}"</div>
                </div>
              </div>
              <div style={{ padding: '14px 18px', borderTop: '1px solid var(--line)', fontSize: 13.5, lineHeight: 1.5, color: '#6b5f50' }}>
                {culture.why}
              </div>
            </div>
          </div>
        )}

        <div className="card row" style={{ padding: '16px 20px', gap: 14, background: 'var(--clay-tint)', borderColor: '#F0DAC8', flexWrap: 'wrap' }}>
          <div style={{ flexGrow: 1, minWidth: 220 }}>
            <div style={{ fontWeight: 600, fontSize: 16 }}>Ready to check your progress?</div>
            <div className="muted" style={{ fontSize: 13.5, marginTop: 2 }}>Take a 4-question check-in to see if you're feeling more comfortable.</div>
          </div>
          <button className="btn-primary" style={{ borderRadius: 14 }} onClick={onQuiz}>
            Take the quiz <ArrowRight size={17} />
          </button>
        </div>

        <div className="row" style={{ gap: 12, marginTop: 2, flexWrap: 'wrap' }}>
          <button className="btn-ghost" style={{ flexGrow: 1 }} onClick={onChallenge}>Get a real-world challenge</button>
          <button className="btn-ghost" onClick={onAgain}>Practice again</button>
          <button className="btn-ghost" onClick={onHome}>Home</button>
        </div>
      </div>
    </div>
  )
}
