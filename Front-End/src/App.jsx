import { useEffect, useState } from 'react'
import Sidebar from './components/Sidebar.jsx'
import Onboarding from './screens/Onboarding.jsx'
import Home from './screens/Home.jsx'
import Briefing from './screens/Briefing.jsx'
import Chat from './screens/Chat.jsx'
import Debrief from './screens/Debrief.jsx'
import Challenge from './screens/Challenge.jsx'
import Quiz from './screens/Quiz.jsx'
import Progress from './screens/Progress.jsx'
import { actors as actorStyles } from './data.js'
import { bridgeApi } from './api.js'

const USER_KEY = 'bridge_user_id'
const actorKeyFor = (type) => type === 'professor' ? 'professors' : 'friends'
const actorTypeFor = (key) => key === 'professors' ? 'professor' : 'friend'

function profileFromUser(user) {
  if (!user?.profile) return null
  return {
    displayName: user.displayName,
    origin: user.profile.countryOfOrigin,
    time: user.profile.usExperience,
    confidence: user.profile.currentConfidence,
    baselineConfidence: user.profile.baselineConfidence,
    recommended: actorKeyFor(user.profile.hardestActorType),
    ratings: {
      class: user.profile.classroomComfort,
      disagree: user.profile.disagreementComfort,
      smalltalk: user.profile.smallTalkComfort,
    },
  }
}

function buildCatalog(apiActors, apiScenarios) {
  return apiActors.reduce((catalog, apiActor) => {
    const key = actorKeyFor(apiActor.actorType)
    const visual = actorStyles[key]
    if (!visual) return catalog
    catalog[key] = {
      ...visual,
      id: apiActor.id,
      actorType: apiActor.actorType,
      persona: {
        ...visual.persona,
        name: apiActor.name,
        role: apiActor.role,
      },
      scenarios: apiScenarios
        .filter((scenario) => scenario.actorId === apiActor.id)
        .map((scenario) => ({
          id: scenario.id,
          title: scenario.title,
          desc: scenario.description,
          level: scenario.difficulty.charAt(0).toUpperCase() + scenario.difficulty.slice(1),
          scene: scenario.description,
          goal: scenario.goal,
          opener: scenario.openingMessage,
          cultureContext: scenario.cultureContext,
        })),
    }
    return catalog
  }, {})
}

export default function App() {
  const [userId, setUserId] = useState(() => localStorage.getItem(USER_KEY))
  const [profile, setProfile] = useState(null)
  const [catalog, setCatalog] = useState(null)
  const [screen, setScreen] = useState('loading')
  const [selected, setSelected] = useState(null)
  const [session, setSession] = useState(null)
  const [evaluation, setEvaluation] = useState(null)
  const [error, setError] = useState('')

  async function loadCatalog() {
    const [apiActors, apiScenarios] = await Promise.all([
      bridgeApi.getActors(),
      bridgeApi.getScenarios(),
    ])
    const next = buildCatalog(apiActors, apiScenarios)
    setCatalog(next)
    return next
  }

  useEffect(() => {
    let active = true
    async function bootstrap() {
      if (!userId) {
        if (active) setScreen('onboarding')
        return
      }
      try {
        const [user] = await Promise.all([bridgeApi.getUser(userId), loadCatalog()])
        if (!active) return
        const savedProfile = profileFromUser(user)
        setProfile(savedProfile)
        setScreen(savedProfile ? 'home' : 'onboarding')
      } catch (requestError) {
        if (!active) return
        setError(requestError.message)
        setScreen('error')
      }
    }
    bootstrap()
    return () => { active = false }
  }, [])

  async function finishOnboarding(localProfile) {
    setError('')
    let currentUserId = userId
    if (!currentUserId) {
      const user = await bridgeApi.createAnonymousUser('Learner')
      currentUserId = user.id
      localStorage.setItem(USER_KEY, currentUserId)
      setUserId(currentUserId)
    }

    const result = await bridgeApi.saveOnboarding(currentUserId, {
      displayName: 'Learner',
      countryOfOrigin: localProfile.origin,
      usExperience: localProfile.time,
      classroomComfort: localProfile.ratings.class,
      disagreementComfort: localProfile.ratings.disagree,
      smallTalkComfort: localProfile.ratings.smalltalk,
      hardestActorType: actorTypeFor(localProfile.recommended),
    })
    await loadCatalog()
    setProfile({
      ...localProfile,
      displayName: 'Learner',
      confidence: result.baselineConfidence,
      baselineConfidence: result.baselineConfidence,
      recommended: actorKeyFor(result.recommendedActorType),
    })
    setScreen('home')
  }

  function retake() {
    setError('')
    setScreen('onboarding')
  }

  function startScenario(actorKey, scenario) {
    setSelected({ actorKey, actor: catalog[actorKey], scenario, directness: 3 })
    setScreen('briefing')
  }

  async function enterChat(directness = selected?.directness || 3) {
    if (!selected || !userId) return
    setError('')
    setScreen('starting')
    try {
      const nextSession = await bridgeApi.createSession({
        userId,
        actorId: selected.actor.id,
        scenarioId: selected.scenario.id,
        directnessLevel: directness,
      })
      setSelected((current) => ({ ...current, directness }))
      setSession(nextSession)
      setScreen('chat')
    } catch (requestError) {
      setError(requestError.message)
      setScreen('briefing')
    }
  }

  function endChat(result) {
    setEvaluation(result)
    setScreen('debrief')
  }

  function completeQuiz(result, nextScreen = 'home') {
    setProfile((current) => current ? { ...current, confidence: result.confidenceAfter } : current)
    setScreen(nextScreen)
  }

  if (screen === 'loading') return <StatusScreen title="Loading your practice space" />
  if (screen === 'error') return <StatusScreen title="Bridge could not reach the API" message={error} action={() => window.location.reload()} />
  if (screen === 'onboarding') return <Onboarding onDone={finishOnboarding} />

  const actor = selected?.actor
  return <div className="app">
    <Sidebar activeNav={screen === 'progress' ? 'progress' : 'home'} onNav={(key) => { setError(''); setScreen(key) }} />
    <main className="content">
      {error && <div className="error-banner" role="alert">{error}</div>}
      {screen === 'starting' && <StatusScreen title="Preparing the conversation" compact />}
      {screen === 'home' && <Home profile={profile} catalog={catalog} onStart={startScenario} onRetake={retake} />}
      {screen === 'progress' && <Progress userId={userId} onContinue={() => setScreen('home')} />}
      {screen === 'briefing' && actor && <Briefing actor={actor} scenario={selected.scenario} onBack={() => setScreen('home')} onStart={enterChat} />}
      {screen === 'chat' && actor && session && <Chat actor={actor} scenario={selected.scenario} session={session} onEnd={endChat} />}
      {screen === 'debrief' && actor && <Debrief actor={actor} scenario={selected.scenario} evaluation={evaluation} onChallenge={() => setScreen('challenge')} onQuiz={() => setScreen('quiz')} onAgain={() => enterChat(selected.directness)} onHome={() => setScreen('home')} />}
      {screen === 'quiz' && session && <Quiz profile={profile} sessionId={session.id} weakness={evaluation?.weakestSkill} onDone={(result) => completeQuiz(result)} onPractice={async (result) => { completeQuiz(result, 'starting'); await enterChat(selected.directness) }} />}
      {screen === 'challenge' && <Challenge actor={actor} onBack={() => setScreen('home')} />}
    </main>
  </div>
}

function StatusScreen({ title, message, action, compact = false }) {
  return <div className={compact ? 'status-screen compact' : 'status-screen'}>
    <div className="status-pulse" />
    <h1>{title}</h1>
    {message && <p>{message}</p>}
    {action && <button className="btn-primary" onClick={action}>Try again</button>}
  </div>
}