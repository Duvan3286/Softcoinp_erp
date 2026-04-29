import * as React from "react";

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "success" | "danger" | "ghost";
}

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className = "", variant = "primary", ...props }, ref) => {
    const variantClass = `btn-${variant}`;
    return (
      <button
        ref={ref}
        className={`${variantClass} ${className}`}
        {...props}
      />
    );
  }
);
Button.displayName = "Button";
