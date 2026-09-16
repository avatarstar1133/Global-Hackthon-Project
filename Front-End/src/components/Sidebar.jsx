import { Bridge, Home, Chart } from './Icons.jsx'

const items = [
  { key: 'home', label: 'Home', Icon: Home },
  { key: 'progress', label: 'Progress', Icon: Chart },
]

export default function Sidebar({ activeNav, onNav }) {
  return <aside className="sidebar">
    <div className="brand"><span className="mark"><Bridge size={19} /></span>Lanco</div>
    <nav className="nav" aria-label="Primary navigation">
      {items.map(({ key, label, Icon }) => (
        <button key={key} className={activeNav === key ? 'active' : ''} onClick={() => onNav(key)}>
          <Icon size={19} />{label}
        </button>
      ))}
    </nav>
  </aside>
}