// Evaluate a whole conversation from its messages.
// Deterministic heuristics for the MVP — this is the seam where a real
// Claude-API evaluation will plug in later.

export function evaluate(messages, actorName = 'them') {
  const userMsgs = messages.filter((m) => m.who === 'me')
  const turns = userMsgs.length
  const hints = messages.filter((m) => m.who === 'tip').length
  const avgLen = turns ? Math.round(userMsgs.reduce((a, m) => a + m.text.length, 0) / turns) : 0
  const askedQuestion = userMsgs.some((m) => m.text.includes('?'))

  const strengths = []
  const toTry = []

  if (turns >= 3) strengths.push('You kept the conversation going for several turns.')
  else if (turns > 0) toTry.push('Try staying in the conversation a little longer next time.')

  if (askedQuestion) strengths.push(`You asked ${actorName} a question back — a natural way to connect.`)
  else toTry.push('Try asking a question back to keep the other person talking.')

  if (avgLen >= 40) strengths.push('Your replies had enough detail to feel warm and open.')
  else toTry.push('Add a bit more detail or a personal note so replies feel less distant.')

  if (hints === 0 && turns > 0) strengths.push('You spoke up on your own, without leaning on hints.')

  if (strengths.length === 0) strengths.push("You showed up and tried — that's the hardest part.")
  if (toTry.length === 0) toTry.push('Keep experimenting with sharing a little more of yourself.')

  let score = 2
  if (turns >= 2) score += 1
  if (askedQuestion) score += 1
  if (avgLen >= 40) score += 1
  score = Math.max(1, Math.min(5, score))

  return { turns, hints, avgLen, askedQuestion, strengths, toTry, score }
}
