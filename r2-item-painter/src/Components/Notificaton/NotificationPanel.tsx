import React from "react";
import { ErrorIcon, InfoIcon, SuccessIcon, WarningIcon } from "../Icons";
import { Button } from "../Components";
import { useNotificationContext, type NotificaitonData } from "./NotificationService";


type NotificationQueueElement = NotificaitonData & {createTime:number}
const MAX_NOTIFS = 5; 
export function NotificationPanel() {
    const [notifQueue, setNotifQueue] = React.useState([] as NotificationQueueElement[]); 
    const notifService = useNotificationContext(); 

    React.useEffect(() => {
        const HandleNotificationReceived = (notif:NotificaitonData) => {
            setNotifQueue(queue => [{...notif, createTime: Date.now()}, ...queue].filter((_, i) => i < MAX_NOTIFS))
        }

        notifService.AddListener(HandleNotificationReceived)
        return () => {
            notifService.RemoveListener(HandleNotificationReceived);
        }
    }, [notifService]); 

    const HandleClose = (createTime:number) => {
        setNotifQueue(queue => queue.filter(qE => qE.createTime != createTime)); 
    }

    return <div className={styles.notifPanel._}>
        {notifQueue.map(qElement => <NotificationItem key={qElement.createTime} data={qElement} OnClose={() => HandleClose(qElement.createTime)} />)}
    </div>
}

const NOTIF_DURATION = 2 * 1000; 
export function NotificationItem(props:{data: NotificaitonData, OnClose:()=>void}) {
    React.useEffect(() => {
        const timeout = setTimeout(props.OnClose, NOTIF_DURATION);
        return () => {
            clearTimeout(timeout); 
        }
    }, [])

    const icon = React.useMemo(() => {
        switch (props.data.status) {
            case 'error': return <ErrorIcon className={styles.notifItem.icons} />;
            case 'success': return <SuccessIcon className={styles.notifItem.icons} />;
            case 'warning': return <WarningIcon className={styles.notifItem.icons} />;
        }

        return <InfoIcon className={styles.notifItem.icons} />;
    }, [props.data.status])

    return <div className={styles.notifItem._}>
        {icon}
        <div className={styles.notifItem.textGroup}>
            <p className={styles.notifItem.title}>{props.data.title}</p>
            {props.data.body && <p className={styles.notifItem.body}>{props.data.body}</p>}
        </div>
        <Button onClick={props.OnClose}>x</Button>
    </div>
}

const styles = {
    notifPanel: {
        _: `flex flex-col-reverse absolute bottom-0 right-0 p-10 w-2/7 gap-2`,
    },
    notifItem: {
        _: `border grid grid-cols-[auto_1fr_auto] grid-rows-1 items-center gap-2 p-2 bg-white relative`,
        textGroup:`grid grid-cols-1 grid-rows-[auto_1fr]`, 
        icons: `w-8 h-8`,
        title: `font-bold`,
        body: `text-small truncate`,
    }
}