import { useState } from 'react'
import Sidebar from './components/Sidebar.jsx'
import Onboarding from './screens/Onboarding.jsx'
import Home from './screens/Home.jsx'
import Briefing from './screens/Briefing.jsx'
import Chat from './screens/Chat.jsx'
import Debrief from './screens/Debrief.jsx'
import Challenge from './screens/Challenge.jsx'
import Quiz from './screens/Quiz.jsx'
import Progress from './screens/Progress.jsx'
import { actors } from './data.js'

function loadProfile() {
  try {
    const raw = localStorage.getItem('bridge_profile')
    if (!raw) return null
    const saved = JSON.parse(raw)
    if (!actors[saved.recommended]) saved.recommended = 'friends'
    return saved
  } catch { return null }
}
function saveHistory(record) {
  try {
    const list = JSON.parse(localStorage.getItem('bridge_history') || '[]')
    list.push(record)
    localStorage.setItem('bridge_history', JSON.stringify(list.slice(-50)))
  } catch { /* ignore local storage failures */ }
}

export default function App() {
  const [profile, setProfile] = useState(loadProfile)
  const [screen, setScreen] = useState(profile ? 'home' : 'onboarding')
  const [selected, setSelected] = useState(null)
  const [evaluation, setEvaluation] = useState(null)
  const [chatKey, setChatKey] = useState(0)

  function persistProfile(next) {
    try { localStorage.setItem('bridge_profile', JSON.stringify(next)) } catch { /* ignore */ }
    setProfile(next)
  }
  function finishOnboarding(next) { persistProfile(next); setScreen('home') }
  function retake() { try { localStorage.removeItem('bridge_profile') } catch { /* ignore */ }; setScreen('onboarding') }
  function startScenario(actorKey, scenario) { setSelected({ actorKey, scenario }); setScreen('briefing') }
  function enterChat() { setChatKey((key) => key + 1); setScreen('chat') }

  function endChat(result, messages) {
    setEvaluation(result)
    const record = {
      id: Date.now(), ts: new Date().toISOString(),
      actor: actors[selected.actorKey].label,
      persona: actors[selected.actorKey].persona.name,
      scenario: selected.scenario.title,
      score: result.score, turns: result.turns,
      weakestSkill: result.weakestSkill,
      dimensions: result.dimensions,
      learningStage: 'reviewed',
      messages,
    }
    saveHistory(record)
    if (profile) persistProfile({ ...profile, focusSkill: result.weakestSkill })
    setScreen('debrief')
  }

  function completeQuiz(newConfidence, correct, nextScreen = 'home') {
    if (profile) persistProfile({
      ...profile,
      confidence: newConfidence,
      focusSkill: evaluation?.weakestSkill || profile.focusSkill,
      lastQuiz: { correct, skill: evaluation?.weakestSkill, ts: new Date().toISOString() },
    })
    setScreen(nextScreen)
  }

  if (screen === 'onboarding') return <Onboarding onDone={finishOnboarding} />
  const actor = selected ? actors[selected.actorKey] : null

  return <div className="app">
    <Sidebar activeNav={screen === 'progress' ? 'progress' : 'home'} onNav={(key) => setScreen(key)} />
    <main className="content">
      {screen === 'home' && <Home profile={profile} onStart={startScenario} onRetake={retake} />}
      {screen === 'progress' && <Progress profile={profile} onContinue={() => setScreen('home')} />}
      {screen === 'briefing' && actor && <Briefing actor={actor} scenario={selected.scenario} onBack={() => setScreen('home')} onStart={enterChat} />}
      {screen === 'chat' && actor && <Chat key={chatKey} actor={actor} scenario={selected.scenario} onEnd={endChat} />}
      {screen === 'debrief' && actor && <Debrief actor={actor} scenario={selected.scenario} evaluation={evaluation} onChallenge={() => setScreen('challenge')} onQuiz={() => setScreen('quiz')} onAgain={enterChat} onHome={() => setScreen('home')} />}
      {screen === 'quiz' && <Quiz profile={profile} weakness={evaluation?.weakestSkill || profile?.focusSkill} onDone={(c, q) => completeQuiz(c, q)} onPractice={(c, q) => { completeQuiz(c, q, 'chat'); setChatKey((key) => key + 1) }} />}
      {screen === 'challenge' && <Challenge actor={actor} onBack={() => setScreen('home')} />}
    </main>
  </div>
}
