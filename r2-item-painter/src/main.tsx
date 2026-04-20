import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import {App} from './App.tsx'
import { DataModelProvider } from './Data/DataModelContext'
import { NotificationProvider } from './Components/Notificaton/NotificationService.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <DataModelProvider>
      <NotificationProvider>
          <App />
      </NotificationProvider>
    </DataModelProvider>
  </StrictMode>,
)
