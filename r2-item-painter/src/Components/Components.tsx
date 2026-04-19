import type React from "react"

type ButtonProps = React.PropsWithChildren<React.ComponentPropsWithRef<'button'>>;
type InputProps = React.PropsWithChildren<React.ComponentPropsWithRef<'input'>>;

export function Button({className, ...rest}:ButtonProps) {
    return <button className={`${style.button._} ${className}`} {...rest} />
}

export function Input({className, ...rest}:InputProps) {
    return <input className={`${style.input._} ${className}`} {...rest} />
}

const style = {
    button: {
        _: `
        border 
        px-4 py-2
        hover:bg-gray-200
        active:bg-gray-100
        `
    }, 

    input: {
        _: `border`, 
    }
}