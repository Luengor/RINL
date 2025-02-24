import { Route, Routes } from "react-router-dom"

import Login from "./pages/login"
import Media from "./pages/media" 

function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/media" element={<Media />} />
    </Routes>
  )
}

export default App

