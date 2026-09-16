import { Bridge, Home, Chart } from './Icons.jsx'
const items = [
  { key: 'home', label: 'Home', Icon: Home },
  { key: 'progress', label: 'Progress', Icon: Chart },
]
export default function Sidebar({ activeNav, onNav }) {
  return <aside className="sidebar">
    <div className="brand"><span className="mark"><Bridge size={19}/></span>Bridge</div>
    <nav className="nav">{items.map(({key,label,Icon}) => <button key={key} className={activeNav===key?'active':''} onClick={()=>onNav(key)}><Icon size={19}/>{label}</button>)}</nav>
    <div className="user"><div className="avatar" style={{background:'var(--clay)'}}>M</div><div><div style={{fontWeight:600,fontSize:14}}>Minh</div><div className="muted" style={{fontSize:12}}>First-year student</div></div></div>
  </aside>
}
