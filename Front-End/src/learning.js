export const skillLabels = {
  clarity: 'Clear expression',
  directness: 'Direct but respectful',
  warmth: 'Warmth and openness',
  engagement: 'Keeping conversation going',
  goalCompletion: 'Completing your goal',
}

const videos = {
  clarity: [
    { title: 'Think Fast, Talk Smart', source: 'Stanford Graduate School of Business', url: 'https://www.youtube.com/watch?v=HAnw168huqA', why: 'Structure spontaneous answers so your point is easier to follow.' },
    { title: 'How to Speak So That People Want to Listen', source: 'TED', url: 'https://www.youtube.com/watch?v=eIho2S0ZahI', why: 'Practice concise, intentional speaking.' },
  ],
  directness: [
    { title: 'How to Disagree Productively', source: 'TED · Julia Dhar', url: 'https://www.youtube.com/watch?v=phgjouv0BUA', why: 'State a different view while preserving common ground.' },
    { title: 'Think Fast, Talk Smart', source: 'Stanford Graduate School of Business', url: 'https://www.youtube.com/watch?v=HAnw168huqA', why: 'Make your request or opinion easier to understand.' },
  ],
  warmth: [
    { title: 'How to Speak So That People Want to Listen', source: 'TED', url: 'https://www.youtube.com/watch?v=eIho2S0ZahI', why: 'Use empathy and vocal choices that invite connection.' },
    { title: '10 Ways to Have a Better Conversation', source: 'TED · Celeste Headlee', url: 'https://www.youtube.com/watch?v=R1vskiVDwl4', why: 'Balance honesty, brevity and listening.' },
  ],
  engagement: [
    { title: '10 Ways to Have a Better Conversation', source: 'TED · Celeste Headlee', url: 'https://www.youtube.com/watch?v=R1vskiVDwl4', why: 'Ask better follow-ups and listen for what to say next.' },
    { title: 'How to Have a Good Conversation', source: 'TEDx · Celeste Headlee', url: 'https://www.youtube.com/watch?v=H6n3iNh4XLI', why: 'Balance talking and listening across several turns.' },
  ],
  goalCompletion: [
    { title: 'Think Fast, Talk Smart', source: 'Stanford Graduate School of Business', url: 'https://www.youtube.com/watch?v=HAnw168huqA', why: 'Organize what you need to say before the moment arrives.' },
    { title: 'How to Disagree Productively', source: 'TED · Julia Dhar', url: 'https://www.youtube.com/watch?v=phgjouv0BUA', why: 'Move from a concern toward a concrete next step.' },
  ],
}

const quizBank = {
  clarity: [
    ['Which answer is clearest?', ['Maybe there is one thing.', 'I need two more days, until Friday.', 'It is a little difficult.'], 1, 'A concrete request and date remove guesswork.'],
    ['A clear answer usually starts with &', ['The main point', 'A long apology', 'Every background detail'], 0, 'Lead with the point, then add useful context.'],
    ['Which detail helps most?', ['A specific reason', 'Repeated  sorry ', 'Unrelated history'], 0, 'One relevant reason supports your request.'],
  ],
  directness: [
    ['Which disagreement is direct and warm?', ['Whatever you want.', 'I see it differently because &', 'That is completely wrong.'], 1, 'Acknowledge the other view, then state yours.'],
    ['Which request is easiest to answer?', ['Maybe if possible &', 'Could we meet Thursday at 2?', 'I hope something can happen.'], 1, 'A specific ask invites a specific response.'],
    ['Direct communication in the U.S. means &', ['Being rude', 'Saying your purpose clearly and respectfully', 'Talking loudly'], 1, 'Directness is about clarity, not aggression.'],
  ],
  warmth: [
    ['Which reply sounds warmer?', ['Yes.', 'Yeah, I m new too   how about you?', 'Correct.'], 1, 'A small detail plus a question creates warmth.'],
    ['Warmth can be shown by &', ['Reciprocal interest', 'Agreeing with everything', 'Repeated apologies'], 0, 'Interest and responsiveness matter more than agreement.'],
    ['Which opener is natural?', ['Do not disturb me.', 'Hey, how s your week going?', 'State your business.'], 1, 'A light opener lowers social pressure.'],
  ],
  engagement: [
    ['How do you keep a conversation moving?', ['Ask a related follow-up', 'Give one-word answers', 'Change topic immediately'], 0, 'Follow-ups show you listened and invite another turn.'],
    ['A good follow-up responds to &', ['What the person just said', 'A memorized script only', 'Nothing in particular'], 0, 'Use their last detail as your bridge.'],
    ['When should you share about yourself?', ['Never', 'Briefly, then return the question', 'For the entire conversation'], 1, 'Balanced sharing keeps the exchange mutual.'],
  ],
  goalCompletion: [
    ['Before speaking, identify &', ['The outcome you need', 'Every possible mistake', 'How to avoid the topic'], 0, 'A clear outcome guides the conversation.'],
    ['A strong request includes &', ['A concrete next step', 'Only an apology', 'No details'], 0, 'A next step lets the other person respond.'],
    ['If the answer is unclear, you should &', ['Confirm what happens next', 'Pretend to understand', 'Leave immediately'], 0, 'Confirmation closes the communication loop.'],
  ],
}

export function resourcesFor(skill) {
  return videos[skill] || videos.engagement
}

export function quizFor(skill) {
  return (quizBank[skill] || quizBank.engagement).map(([q, options, correct, why]) => ({
    q,
    options: options.map((text, index) => ({ text, correct: index === correct })),
    why,
  }))
}
