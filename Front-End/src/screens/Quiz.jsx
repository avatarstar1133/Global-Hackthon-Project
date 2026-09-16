import { useState } from 'react'
import { quizFor, skillLabels } from '../learning.js'
import { Check, Warning, ArrowRight, Sparkle } from '../components/Icons.jsx'

const feelLabels = ['Not yet', 'A little', 'Okay', 'Pretty comfortable', 'Confident']

export default function Quiz({ profile, weakness = 'engagement', onDone, onPractice }) {
  const questions = quizFor(weakness)
  const [step, setStep] = useState(0)
  const [picked, setPicked] = useState(null)
  const [correct, setCorrect] = useState(0)
  const [result, setResult] = useState(null)

  function pick(idx) {
    if (picked !== null) return
    setPicked(idx)
    if (questions[step].options[idx].correct) setCorrect((v) => v + 1)
  }

  if (result) {
    const old = profile?.confidence ?? 3
    const up = result.newConfidence > old
    return <div className="center-wrap"><div className="center-col" style={{maxWidth:540,textAlign:'center'}}>
      <div className="row" style={{justifyContent:'center',gap:8,color:'var(--clay)'}}><Sparkle size={20}/><span className="label">LEARNING CHECK COMPLETE</span></div>
      <h1 style={{fontSize:28}}>{up ? "You're feeling more ready" : 'Now test it in a real conversation'}</h1>
      <div className="card" style={{padding:20,background:'var(--teal-tint)'}}><b>Quiz: {correct} / {questions.length}</b><div className="muted" style={{marginTop:5}}>Focus: {skillLabels[weakness]}</div></div>
      <div className="card row" style={{padding:20,justifyContent:'center',gap:20}}><div><div className="muted">BEFORE</div><h1>{old}</h1></div><ArrowRight/><div><div className="muted">NOW</div><h1 style={{color:'var(--sage)'}}>{result.newConfidence}</h1></div></div>
      <button className="btn-primary" style={{padding:16,justifyContent:'center'}} onClick={() => onPractice(result.newConfidence, correct)}>Practice this skill again <ArrowRight size={18}/></button>
      <button className="btn-ghost" onClick={() => onDone(result.newConfidence, correct)}>Back to home</button>
    </div></div>
  }

  const isFeeling = step === questions.length
  return <div className="center-wrap"><div className="center-col" style={{maxWidth:570}}>
    <div className="label" style={{textAlign:'center'}}>FOCUS · {skillLabels[weakness]}</div>
    {!isFeeling ? <>
      <h1 style={{fontSize:24,textAlign:'center',lineHeight:1.35}}>{questions[step].q}</h1>
      <div style={{display:'flex',flexDirection:'column',gap:10}}>{questions[step].options.map((o,i)=>{
        const reveal=picked!==null; const chosen=picked===i
        return <button key={i} className="card row" onClick={()=>pick(i)} style={{padding:'15px 18px',textAlign:'left',background:reveal&&o.correct?'var(--sage-tint)':reveal&&chosen?'#F7E4DA':'var(--surface)',borderColor:reveal&&o.correct?'var(--sage)':reveal&&chosen?'var(--clay)':'var(--line)'}}>
          <span style={{flex:1,fontWeight:500}}>{o.text}</span>{reveal&&o.correct&&<Check size={18}/>} {reveal&&chosen&&!o.correct&&<Warning size={18}/>}
        </button>})}</div>
      {picked!==null&&<><div className="card muted" style={{padding:14,fontSize:13.5}}>{questions[step].why}</div><button className="btn-primary" style={{justifyContent:'center'}} onClick={()=>{setPicked(null);setStep(v=>v+1)}}>Next <ArrowRight size={17}/></button></>}
    </> : <>
      <h1 style={{fontSize:25,textAlign:'center'}}>How ready do you feel to try this skill again?</h1>
      {feelLabels.map((label,i)=><button key={label} className="card row" style={{padding:15}} onClick={()=>setResult({newConfidence:i+1})}><span className="badge">{i+1}</span><b>{label}</b></button>)}
    </>}
  </div></div>
}
