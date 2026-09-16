const API_BASE = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '')

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  const contentType = response.headers.get('content-type') || ''
  const body = contentType.includes('application/json')
    ? await response.json()
    : await response.text()

  if (!response.ok) {
    const message = body?.error || body?.detail || body?.title || body || `Request failed (${response.status})`
    throw new Error(message)
  }

  return body
}

export const bridgeApi = {
  createAnonymousUser: (displayName = 'Learner') => request('/users/anonymous', {
    method: 'POST',
    body: JSON.stringify({ displayName }),
  }),
  getUser: (userId) => request(`/users/${userId}`),
  saveOnboarding: (userId, assessment) => request('/onboarding/assess', {
    method: 'POST',
    body: JSON.stringify({ userId, assessment }),
  }),
  getActors: () => request('/actors'),
  getScenarios: () => request('/scenarios'),
  createSession: (payload) => request('/sessions', {
    method: 'POST',
    body: JSON.stringify(payload),
  }),
  getSession: (sessionId) => request(`/sessions/${sessionId}`),
  sendMessage: (sessionId, content) => request(`/sessions/${sessionId}/messages`, {
    method: 'POST',
    body: JSON.stringify({ clientMessageId: crypto.randomUUID(), content }),
  }),
  getHint: (sessionId) => request(`/sessions/${sessionId}/hint`, { method: 'POST' }),
  completeSession: (sessionId) => request(`/sessions/${sessionId}/complete`, {
    method: 'POST',
    body: JSON.stringify({ confirmed: true }),
  }),
  getLearningPlan: (sessionId) => request(`/sessions/${sessionId}/learning-plan`),
  getQuiz: (sessionId) => request(`/sessions/${sessionId}/quiz`, { method: 'POST' }),
  submitQuiz: (sessionId, answers, confidenceAfter) => request(`/sessions/${sessionId}/quiz/submit`, {
    method: 'POST',
    body: JSON.stringify({ answers, confidenceAfter }),
  }),
  getProgress: (userId) => request(`/progress/${userId}`),
}