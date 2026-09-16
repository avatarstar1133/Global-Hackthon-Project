export function evaluate(messages, actorName = 'them') {
  const userMsgs = messages.filter((m) => m.who === 'me')
  const turns = userMsgs.length
  const hints = messages.filter((m) => m.who === 'tip').length
  const avgLen = turns ? Math.round(userMsgs.reduce((sum, m) => sum + m.text.length, 0) / turns) : 0
  const askedQuestion = userMsgs.some((m) => m.text.includes('?'))
  const hedges = userMsgs.filter((m) => /\b(maybe|perhaps|sorry|if possible|i think maybe)\b/i.test(m.text)).length

  const dimensions = {
    clarity: clamp(1 + (avgLen >= 20 ? 1 : 0) + (avgLen >= 45 ? 1 : 0) + (turns >= 2 ? 1 : 0)),
    directness: clamp(4 - Math.min(2, hedges) + (avgLen >= 25 ? 1 : 0)),
    warmth: clamp(2 + (askedQuestion ? 1 : 0) + (avgLen >= 35 ? 1 : 0)),
    engagement: clamp(1 + Math.min(3, turns) + (askedQuestion ? 1 : 0)),
    goalCompletion: clamp(1 + Math.min(3, turns) + (avgLen >= 40 ? 1 : 0)),
  }

  const weakestSkill = Object.entries(dimensions).sort((a, b) => a[1] - b[1])[0][0]
  const strengths = []
  const toTry = []

  if (turns >= 3) strengths.push('You kept the conversation going for several turns.')
  else if (turns > 0) toTry.push('Stay in the conversation for one more turn next time.')

  if (askedQuestion) strengths.push(`You asked ${actorName} a question back   a natural way to connect.`)
  else toTry.push('Ask a related question back to keep the exchange moving.')

  if (avgLen >= 40) strengths.push('Your replies included enough detail to feel open and clear.')
  else toTry.push('Add one specific detail so the other person knows how to respond.')

  if (hedges > 0) toTry.push('State the main request or opinion before apologizing or softening it.')
  if (hints === 0 && turns > 0) strengths.push('You spoke up without relying on hints.')
  if (!strengths.length) strengths.push("You showed up and tried   that is the first rep.")
  if (!toTry.length) toTry.push('Keep practicing the same skill in a slightly harder scenario.')

  const score = clamp(Math.round(Object.values(dimensions).reduce((a, b) => a + b, 0) / 5))
  return { turns, hints, avgLen, askedQuestion, strengths, toTry, score, dimensions, weakestSkill }
}

function clamp(value) {
  return Math.max(1, Math.min(5, value))
}
