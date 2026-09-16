// ---- Intake assessment ----------------------------------------------------
// Shown when the user first enters, to understand where they are and evaluate
// their current comfort. The result becomes their profile.

export const originQuestion = {
  id: 'origin',
  q: 'Where did you grow up?',
  options: ['China', 'South Korea', 'India', 'Somewhere else'],
}

export const timeQuestion = {
  id: 'time',
  q: 'How long have you been studying in the United States of America?',
  options: ['I just arrived', 'Less than a semester', '1–2 semesters', 'Over a year'],
}

// 1 (very hard) → 4 (easy for me)
export const scaleQuestions = [
  { id: 'class', q: 'Speaking up in class feels…' },
  { id: 'disagree', q: 'Telling someone I disagree feels…' },
  { id: 'smalltalk', q: 'Making small talk with strangers feels…' },
]
export const scaleLabels = ['Very hard', 'Hard', 'Okay', 'Easy for me']

// ---- Actors (all American) -------------------------------------------------

export const actors = {
  friends: {
    key: 'friends',
    label: 'Friends',
    tagline: 'Peers & classmates',
    color: 'var(--clay)',
    tint: 'var(--clay-tint)',
    border: '#F0DAC8',
    about: 'Casual American peers. Learn to start conversations, joke around, and share opinions without sounding distant.',
    persona: { name: 'Jake', role: 'Sophomore · friendly and easygoing', initial: 'J', accent: 'var(--teal)' },
    scenarios: [
      {
        id: 'f-class', title: 'Start a conversation in class', desc: 'Break the ice with a classmate.', level: 'Easy',
        scene: 'You just sat down next to another student who is waiting for the lecture to begin.',
        goal: "Learn Jake's name and one thing about him.",
        opener: 'Hey! Is this seat taken? First time in this class too?',
        replies: [
          "Nice! CS is tough but fun. I'm doing Psychology, actually.",
          'Oh cool. Where are you from originally?',
          "That's awesome. Have you found any good coffee spots on campus yet?",
          'Ha, same here. Want to grab a seat together next class?',
        ],
        culture: {
          said: 'Yes, first time. I am also new here.',
          natural: "Yeah, first time! I'm new here too — kind of nervous, honestly.",
          why: 'Americans often add a small feeling or detail to sound warm and open, not distant.',
        },
      },
      {
        id: 'f-party', title: 'Small talk at a party', desc: 'Keep a light conversation going.', level: 'Easy',
        scene: "You just arrived at a classmate's birthday party where you barely know anyone.",
        goal: 'Keep a friendly conversation going for a few turns.',
        opener: "Hey, I don't think we've met! How do you know the host?",
        replies: [
          'Oh nice! Are you in the same major?',
          "Cool. Have you tried the food yet? It's really good.",
          'Haha same. So what do you do for fun around here?',
          'Nice talking to you! Let’s grab a table.',
        ],
        culture: {
          said: 'I know the host from class.',
          natural: 'Oh, I know them from class — we had Bio together last term. How about you?',
          why: 'Bouncing the question back keeps small talk alive instead of ending it.',
        },
      },
      {
        id: 'f-disagree', title: 'Say "I disagree" with a friend', desc: 'Share a different opinion, stay warm.', level: 'Medium',
        scene: "Your friend suggests a plan you don't really like.",
        goal: 'Voice a different opinion while keeping it friendly.',
        opener: "Let's just skip the study group and cram tonight, yeah?",
        replies: [
          'Ha, fair. What makes you want to skip it?',
          'Hmm, I see your point though.',
          'Okay, maybe we meet halfway?',
          'Cool, thanks for being honest with me.',
        ],
        culture: {
          said: "Okay, that's fine.",
          natural: "Hmm, I'd actually rather keep the study group — is that okay with you?",
          why: 'Stating a preference directly is normal here and not seen as rude.',
        },
      },
    ],
  },

  professors: {
    key: 'professors',
    label: 'Professors',
    tagline: 'Faculty & authority',
    color: 'var(--teal-d)',
    tint: 'var(--teal-tint)',
    border: '#D9E8E3',
    about: 'American faculty value directness and initiative. Learn to speak up, use office hours, and handle feedback with confidence.',
    persona: { name: 'Dr. Miller', role: 'Your course professor', initial: 'M', accent: 'var(--teal-d)' },
    scenarios: [
      {
        id: 'p-office', title: 'Introduce yourself in office hours', desc: 'Approach a professor for the first time.', level: 'Medium',
        scene: "You walk into your professor's office hours for the first time.",
        goal: 'Introduce yourself and ask one question about the class.',
        opener: 'Hi, come on in! What can I help you with today?',
        replies: [
          "Great to meet you. Which part of the material feels tricky?",
          "That's a good question — let's look at it together.",
          'Absolutely, feel free to come by anytime.',
          'Glad you stopped by. Anything else on your mind?',
        ],
        culture: {
          said: 'Sorry to bother you, I have a small question.',
          natural: "Hi Dr. Miller, I'm Minh from your Tuesday class — I had a question about the reading.",
          why: 'Professors expect you to introduce yourself and ask directly. It is not bothering them.',
        },
      },
      {
        id: 'p-extension', title: 'Ask for a deadline extension', desc: 'Make a clear, polite request.', level: 'Hard',
        scene: 'You need two more days on an assignment and have to ask your professor.',
        goal: 'Clearly and politely ask for an extension, with a reason.',
        opener: 'Hi, you wanted to talk about the assignment?',
        replies: [
          "I see. Can you tell me a bit more about what's going on?",
          'I appreciate you letting me know early.',
          "Let's find a deadline that works for you.",
          'Sure — just email me to confirm.',
        ],
        culture: {
          said: 'I am very sorry, maybe if possible could I please have more time?',
          natural: "I've been unwell this week. Could I have two more days to submit my best work?",
          why: 'A clear reason plus a specific ask works better than repeated apologies.',
        },
      },
      {
        id: 'p-feedback', title: 'Respond to critical feedback', desc: "Don't shut down — engage.", level: 'Hard',
        scene: 'Your professor gives you tough feedback on your draft.',
        goal: 'Respond openly without shutting down, and ask a follow-up.',
        opener: "Your argument is interesting, but the evidence is thin. What's your thinking here?",
        replies: [
          "That's a reasonable start. How could you strengthen it?",
          "Good — that shows you're engaging with it.",
          "Exactly. Don't be afraid to push your point.",
          'I look forward to the revision.',
        ],
        culture: {
          said: "Okay. I'm sorry.",
          natural: 'That’s helpful — could you point me to where the evidence felt weakest?',
          why: 'Asking a follow-up shows engagement; going quiet can read as disengagement.',
        },
      },
    ],
  },

}

export const actorOrder = ['friends', 'professors']

// ---- Re-check quiz --------------------------------------------------------
// A short knowledge check + self re-rating to see whether the user is
// getting more comfortable since their first check-in.

export const quizQuestions = [
  {
    q: "A classmate you barely know says \"How's it going?\" as they pass by. The most natural reply is:",
    options: [
      { text: 'Give a detailed, honest update about your day', correct: false },
      { text: '"Pretty good, you?" and keep it light', correct: true },
      { text: 'Smile and stay silent', correct: false },
    ],
    why: 'In the US, "How\'s it going?" is usually a friendly greeting, not a real question — a short, warm reply is expected.',
  },
  {
    q: "You disagree with a friend's plan. The most natural approach is:",
    options: [
      { text: 'Say nothing to keep the peace', correct: false },
      { text: '"I see your point, but I\'d actually prefer…"', correct: true },
      { text: 'Quietly change your own plans to match', correct: false },
    ],
    why: 'Stating a preference directly, while acknowledging theirs, is normal and not rude here.',
  },
  {
    q: 'Your professor gives you tough feedback on a draft. A strong response is:',
    options: [
      { text: 'Apologize a lot and go quiet', correct: false },
      { text: 'Ask a follow-up question to understand and improve', correct: true },
      { text: 'Defend every single point', correct: false },
    ],
    why: 'Engaging with a follow-up question shows you take feedback well — silence can look like disengagement.',
  },
]
