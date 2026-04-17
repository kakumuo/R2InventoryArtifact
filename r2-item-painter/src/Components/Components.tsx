import type React from "react"

type ButtonProps = React.PropsWithChildren<React.ComponentPropsWithoutRef<'button'>>;
type InputProps = React.PropsWithChildren<React.ComponentPropsWithoutRef<'input'>>;

export function Button({className, ...rest}:ButtonProps) {
    return <button className={`${style.button._} ${className}`} {...rest} />
}

export function Input({className, ...rest}:InputProps) {
    return <input className={`${style.input._} ${className}`} {...rest} />
}

const style = {
    button: {
        _: `border`
    }, 

    input: {
        _: `border`, 
    }
}