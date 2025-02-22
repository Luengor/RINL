import { LoginForm } from "./loginForm"
import { Link } from "react-router-dom"

function Login() {
  return (
    <>
      <LoginForm registrationLink={<Link to={'/register'}>Mala suerte</Link>}/>
    </>
  )
}

export default Login 
