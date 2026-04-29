import * as React from "react";

export type InputProps = React.InputHTMLAttributes<HTMLInputElement>;

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className = "", type, ...props }, ref) => {
    return (
      <input
        type={type}
        className={`input-standard ${className}`}
        ref={ref}
        {...props}
      />
    );
  }
);
Input.displayName = "Input";
