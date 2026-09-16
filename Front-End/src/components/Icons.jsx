// Stroke-based inline icons. Pass size + any svg props.
const S = (p) => ({
  width: p.size || 20, height: p.size || 20, viewBox: '0 0 24 24',
  fill: 'none', stroke: 'currentColor', strokeWidth: p.sw || 1.9,
  strokeLinecap: 'round', strokeLinejoin: 'round',
  style: p.style, ...p.rest
})

export const Bridge = (p) => (<svg {...S({ ...p, sw: 2 })}><path d="M3 15c3-5 5-5 9 0s6 5 9 0"/><path d="M3 9h18"/></svg>)
export const Home = (p) => (<svg {...S(p)}><path d="M3 10.5 12 3l9 7.5"/><path d="M5 9.5V21h14V9.5"/></svg>)
export const Chart = (p) => (<svg {...S(p)}><path d="M4 20V4M4 20h16M8 16v-4M13 16V8M18 16v-7"/></svg>)
export const Decoder = (p) => (<svg {...S(p)}><path d="M4 5h7M9 3v2c0 4-2 7-5 8"/><path d="M5 9c0 2.5 2.5 4.5 6 5.5M12 20l4-9 4 9M13.5 17h5"/></svg>)
export const Flame = (p) => (<svg {...S(p)}><path d="M12 3c1.5 3 4 4.5 4 8a4 4 0 1 1-8 0c0-1.2.4-2.2 1-3 .3 1 .8 1.6 1.6 2 .1-2.6-1-4.6.4-7Z"/></svg>)
export const Speech = (p) => (<svg {...S(p)}><path d="M21 11.5a8.5 8.5 0 0 1-12.2 7.7L3 21l1.9-5.7A8.5 8.5 0 1 1 21 11.5Z"/></svg>)
export const Target = (p) => (<svg {...S(p)}><circle cx="12" cy="12" r="9"/><circle cx="12" cy="12" r="4.5"/><circle cx="12" cy="12" r="1"/></svg>)
export const TargetDot = (p) => (<svg {...S(p)}><circle cx="12" cy="12" r="9"/><circle cx="12" cy="12" r="1.5"/></svg>)
export const Users = (p) => (<svg {...S(p)}><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/></svg>)
export const Megaphone = (p) => (<svg {...S(p)}><path d="M3 11l18-5v12L3 14v-3Z"/><path d="M11.6 16.8A3 3 0 0 1 6 15.5"/></svg>)
export const Coffee = (p) => (<svg {...S(p)}><path d="M18 8h1a4 4 0 0 1 0 8h-1"/><path d="M2 8h16v9a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V8Z"/><path d="M6 2v2M10 2v2M14 2v2"/></svg>)
export const Cap = (p) => (<svg {...S(p)}><path d="M22 10 12 5 2 10l10 5 10-5Z"/><path d="M6 12v5c0 1 2.7 3 6 3s6-2 6-3v-5"/></svg>)
export const Bulb = (p) => (<svg {...S(p)}><path d="M9 18h6M10 22h4M12 2a7 7 0 0 0-4 12.7c.6.5 1 1.2 1 2h6c0-.8.4-1.5 1-2A7 7 0 0 0 12 2Z"/></svg>)
export const Send = (p) => (<svg {...S({ ...p, sw: 2 })}><path d="M22 2 11 13M22 2l-7 20-4-9-9-4 20-7Z"/></svg>)
export const ArrowRight = (p) => (<svg {...S({ ...p, sw: 2.2 })}><path d="M5 12h14M13 6l6 6-6 6"/></svg>)
export const ChevronLeft = (p) => (<svg {...S({ ...p, sw: 2 })}><path d="M15 18l-6-6 6-6"/></svg>)
export const Check = (p) => (<svg {...S({ ...p, sw: p.sw || 3 })}><path d="M20 6 9 17l-5-5"/></svg>)
export const Plus = (p) => (<svg {...S({ ...p, sw: 3 })}><path d="M12 5v14M5 12h14"/></svg>)
export const Shield = (p) => (<svg {...S(p)}><path d="M12 2 4 6v6c0 5 3.5 8 8 10 4.5-2 8-5 8-10V6l-8-4Z"/><path d="M9 12l2 2 4-4"/></svg>)
export const Warning = (p) => (<svg {...S({ ...p, sw: 2 })}><path d="M12 9v4M12 17h.01M10.3 3.9 2.4 18a2 2 0 0 0 1.7 3h15.8a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0Z"/></svg>)
export const Sparkle = (p) => (<svg {...S(p)}><path d="M12 3v2M18.4 5.6l-1.4 1.4M21 12h-2M5 12H3M7 7 5.6 5.6M12 8a4 4 0 0 0-2 7.5V18h4v-2.5A4 4 0 0 0 12 8Z"/><path d="M10 21h4"/></svg>)
