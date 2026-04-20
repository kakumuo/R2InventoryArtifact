import React, { useContext } from "react";

export type NotificaitonData = {
    title: string,
    body: string|null,
    status: 'info' | 'warning' | 'error' | 'success'
}

// eslint-disable-next-line react-refresh/only-export-components
export class NotificationService {    
    listeners:((notif:NotificaitonData)=>void)[] = []

    public PushNotificaton(data:NotificaitonData) {
        this.InvokeListener(data); 
    }

    public AddListener(listener:(notif:NotificaitonData)=>void) {
        this.listeners.push(listener);
    }

    public RemoveListener(listener:(notif:NotificaitonData)=>void){
        this.listeners = this.listeners.filter(l => l != listener); 
    }

    public InvokeListener(notif:NotificaitonData){
        this.listeners.forEach(l => l(notif)); 
    }
}

const NotificationContext = React.createContext<NotificationService | null>(null); 
const notifService = new NotificationService(); 

// eslint-disable-next-line react-refresh/only-export-components
export const useNotificationContext = () => {
    const notifService = useContext(NotificationContext);   
    if(!notifService) throw new Error("Wrap with NotificationProvider"); 

    return notifService; 
}

export const NotificationProvider = ({children}:{children:React.ReactNode}) => {
    return <NotificationContext.Provider value={notifService}>
        {children}
    </NotificationContext.Provider>
}