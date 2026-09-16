import { useState } from 'react'
import Sidebar from './components/Sidebar.jsx'
import Onboarding from './screens/Onboarding.jsx'
import Home from './screens/Home.jsx'
import Briefing from './screens/Briefing.jsx'
import Chat from './screens/Chat.jsx'
import Debrief from './screens/Debrief.jsx'
import Challenge from './screens/Challenge.jsx'
import Quiz from './screens/Quiz.jsx'
import { actors } from './data.js'

function loadProfile() {
  try {
    const raw = localStorage.getItem('bridge_profile')
    return raw ? JSON.parse(raw) : null
  } catch { return null }
}

function saveHistory(record) {
  try {
    const raw = localStorage.getItem('bridge_history')
    const list = raw ? JSON.parse(raw) : []
    list.push(record)
    localStorage.setItem('bridge_history', JSON.stringify(list.slice(-50)))
  } catch { /* ignore */ }
}

export default function App() {
  const [profile, setProfile] = useState(loadProfile)
  const [screen, setScreen] = useState(profile ? 'home' : 'onboarding')
  const [selected, setSelected] = useState(null)   // { actorKey, scenario }
  const [evaluation, setEvaluation] = useState(null)
  const [chatKey, setChatKey] = useState(0)

  function persistProfile(p) {
    try { localStorage.setItem('bridge_profile', JSON.stringify(p)) } catch { /* ignore */ }
    setProfile(p)
  }

  function finishOnboarding(p) {
    persistProfile(p)
    setScreen('home')
  }

  function retake() {
    try { localStorage.removeItem('bridge_profile') } catch { /* ignore */ }
    setScreen('onboarding')
  }

  const startScenario = (actorKey, scenario) => {
    setSelected({ actorKey, scenario })
    setScreen('briefing')
  }
  const enterChat = () => { setChatKey((k) => k + 1); setScreen('chat') }

  // Chat is done: evaluate the whole conversation and save its context.
  function endChat(evalResult, messages) {
    setEvaluation(evalResult)
    saveHistory({
      id: Date.now(),
      ts: new Date().toISOString(),
      actor: actors[selected.actorKey].label,
      persona: actors[selected.actorKey].persona.name,
      scenario: selected.scenario.title,
      score: evalResult.score,
      turns: evalResult.turns,
      messages,
    })
    setScreen('debrief')
  }

  // Quiz done: update confidence and record the re-check.
  function finishQuiz(newConfidence, correct) {
    if (profile) persistProfile({ ...profile, confidence: newConfidence, lastQuiz: { correct, ts: new Date().toISOString() } })
    setScreen('home')
  }

  if (screen === 'onboarding') return <Onboarding onDone={finishOnboarding} />

  const actor = selected ? actors[selected.actorKey] : null

  return (
    <div className="app">
      <Sidebar activeNav="home" onNav={() => setScreen('home')} />
      <main className="content">
        {screen === 'home' && (
          <Home profile={profile} onStart={startScenario} onRetake={retake} />
        )}
        {screen === 'briefing' && actor && (
          <Briefing actor={actor} scenario={selected.scenario} onBack={() => setScreen('home')} onStart={enterChat} />
        )}
        {screen === 'chat' && actor && (
          <Chat key={chatKey} actor={actor} scenario={selected.scenario} onEnd={endChat} />
        )}
        {screen === 'debrief' && actor && (
          <Debrief actor={actor} scenario={selected.scenario} evaluation={evaluation}
            onChallenge={() => setScreen('challenge')} onQuiz={() => setScreen('quiz')}
            onAgain={enterChat} onHome={() => setScreen('home')} />
        )}
        {screen === 'quiz' && (
          <Quiz profile={profile} onDone={finishQuiz} />
        )}
        {screen === 'challenge' && (
          <Challenge actor={actor} onBack={() => setScreen('home')} />
        )}
      </main>
    </div>
  )
}
