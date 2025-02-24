import { LoginForm } from "../components/loginForm"
import { Link } from "react-router-dom"
import { Center } from "@mantine/core"

function Login() {
  return (
    <Center>
      <LoginForm registrationLink={<Link to={'/register'}>Mala suerte</Link>}/>
    </Center>
  )
}

export default Login 
