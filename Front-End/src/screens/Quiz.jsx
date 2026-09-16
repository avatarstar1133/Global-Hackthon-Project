import { useEffect, useState } from 'react'
import { Check, Warning, ArrowRight, Sparkle } from '../components/Icons.jsx'
import { bridgeApi } from '../api.js'
import { skillLabels } from '../learning.js'

const feelLabels = ['Not yet', 'A little', 'Okay', 'Pretty comfortable', 'Confident']

export default function Quiz({ profile, sessionId, weakness = 'engagement', onDone, onPractice }) {
  const [questions, setQuestions] = useState([])
  const [step, setStep] = useState(0)
  const [answers, setAnswers] = useState([])
  const [picked, setPicked] = useState(null)
  const [result, setResult] = useState(null)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    bridgeApi.getQuiz(sessionId)
      .then((data) => { if (active) setQuestions(data) })
      .catch((requestError) => { if (active) setError(requestError.message) })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [sessionId])

  function nextQuestion() {
    if (picked === null) return
    setAnswers((current) => [...current, picked])
    setPicked(null)
    setStep((current) => current + 1)
  }

  async function submit(confidenceAfter) {
    setSubmitting(true)
    setError('')
    try {
      const response = await bridgeApi.submitQuiz(sessionId, answers, confidenceAfter)
      setResult(response)
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) return <div className="status-screen compact"><div className="status-pulse" /><h1>Loading your quiz</h1></div>
  if (error && !questions.length) return <div className="status-screen compact"><h1>Quiz unavailable</h1><p>{error}</p></div>

  if (result) {
    const oldConfidence = result.confidenceBefore ?? profile?.confidence ?? 3
    const improved = result.confidenceAfter > oldConfidence
    return <div className="center-wrap"><div className="center-col quiz-result">
      <div className="row completion-label"><Sparkle size={20} /><span className="label">Learning check complete</span></div>
      <h1>{improved ? 'You are feeling more ready' : 'Now test it in another conversation'}</h1>
      <div className="quiz-score"><b>{result.score} / {result.maxScore}</b><span>Focus: {skillLabels[weakness] || 'Conversation practice'}</span></div>
      <div className="confidence-shift"><div><span>Before</span><strong>{oldConfidence}</strong></div><ArrowRight /><div><span>Now</span><strong>{result.confidenceAfter}</strong></div></div>
      <div className="answer-review">
        {result.answers.map((answer, index) => <div key={index} className={answer.correct ? 'correct' : 'incorrect'}>
          {answer.correct ? <Check size={17} /> : <Warning size={17} />}
          <span>{answer.explanation}</span>
        </div>)}
      </div>
      <button className="btn-primary wide-button" onClick={() => onPractice(result)}>Practice this skill again <ArrowRight size={18} /></button>
      <button className="btn-ghost" onClick={() => onDone(result)}>Back to home</button>
    </div></div>
  }

  const confidenceStep = step === questions.length
  const question = questions[step]

  return <div className="center-wrap"><div className="center-col quiz-col">
    <div className="label centered-label">Focus · {skillLabels[weakness] || 'Conversation practice'}</div>
    {error && <div className="inline-error" role="alert">{error}</div>}
    {!confidenceStep && question ? <>
      <div className="quiz-progress">Question {step + 1} of {questions.length}</div>
      <h1>{question.prompt}</h1>
      <div className="option-list">
        {question.options.map((option, index) => <button key={option} className={`quiz-option ${picked === index ? 'selected' : ''}`} onClick={() => setPicked(index)}>
          <span>{String.fromCharCode(65 + index)}</span>{option}
        </button>)}
      </div>
      <button className="btn-primary wide-button" disabled={picked === null} onClick={nextQuestion}>Next <ArrowRight size={17} /></button>
    </> : <>
      <h1>How ready do you feel to try this skill again?</h1>
      <div className="feeling-list">
        {feelLabels.map((label, index) => <button key={label} className="quiz-option" disabled={submitting} onClick={() => submit(index + 1)}><span>{index + 1}</span>{label}</button>)}
      </div>
      {submitting && <div className="muted centered-label">Saving your result…</div>}
    </>}
  </div></div>
}