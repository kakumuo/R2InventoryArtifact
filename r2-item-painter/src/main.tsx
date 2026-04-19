import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import {App} from './App.tsx'
import { DataModelProvider } from './Data/DataModelContext'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <DataModelProvider>
      <App />
    </DataModelProvider>
  </StrictMode>,
)
