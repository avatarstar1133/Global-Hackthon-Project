import { useEffect, useState } from 'react'
import { skillLabels } from '../learning.js'
import { ArrowRight, Check, Chart } from '../components/Icons.jsx'
import { bridgeApi } from '../api.js'

export default function Progress({ userId, onContinue }) {
  const [progress, setProgress] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    bridgeApi.getProgress(userId)
      .then((data) => { if (active) setProgress(data) })
      .catch((requestError) => { if (active) setError(requestError.message) })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [userId])

  if (loading) return <div className="status-screen compact"><div className="status-pulse" /><h1>Loading your progress</h1></div>
  if (error) return <div className="status-screen compact"><h1>Progress unavailable</h1><p>{error}</p></div>

  const focus = progress.currentFocus || 'engagement'
  const history = progress.recentSessions || []
  const hasSession = progress.completedSessions > 0
  const quizComplete = progress.quizAttempts > 0
  const cycleDone = [hasSession, hasSession, hasSession, quizComplete, false]

  return <div className="pad progress-page">
    <header className="hdr">
      <div>
        <div className="eyebrow">Your learning path</div>
        <h1>Progress grows one cycle at a time</h1>
        <p className="page-intro">Practice, review one weak point, learn it, then try again in context.</p>
      </div>
    </header>

    <div className="cycle-strip">
      <div className="label">Current cycle</div>
      <div className="cycle-steps">
        {['Practice', 'Review', 'Learn', 'Quiz', 'Practice again'].map((label, index) => <div key={label} className="cycle-step">
          <span className={cycleDone[index] ? 'complete' : ''}>{cycleDone[index] ? <Check size={14} /> : index + 1}</span>
          <b>{label}</b>
          {index < 4 && <ArrowRight size={15} />}
        </div>)}
      </div>
    </div>

    <div className="metrics-grid">
      <div className="focus-metric">
        <div className="label">Current focus</div>
        <h2>{skillLabels[focus] || 'Conversation practice'}</h2>
        <p>Your next conversation should test this skill again in a realistic setting.</p>
        <button className="btn-primary" onClick={onContinue}>Continue learning <ArrowRight size={17} /></button>
      </div>
      <div className="metric-block"><span>Completed sessions</span><strong>{progress.completedSessions}</strong></div>
      <div className="metric-block"><span>Average score</span><strong>{progress.averageScore || '—'}<small>{progress.averageScore ? '/5' : ''}</small></strong></div>
      <div className="metric-block"><span>Confidence</span><strong>{progress.currentConfidence}<small>/5</small></strong><p>Started at {progress.baselineConfidence}/5</p></div>
    </div>

    <section>
      <div className="section-heading compact"><div><div className="label">Recent sessions</div><h2>Your latest evidence</h2></div></div>
      <div className="session-list">
        {!history.length && <div className="empty-state">Complete a conversation to start your progress record.</div>}
        {history.map((item) => <div className="session-row" key={item.id}>
          <span className="ic"><Chart size={19} /></span>
          <div><b>{item.scenario}</b><span>{item.actor} · Focus: {skillLabels[item.focusSkill] || 'Conversation practice'}</span></div>
          <span className="session-score">{item.score}/5</span>
        </div>)}
      </div>
    </section>
  </div>
}