import * as React from "react";

export const Card = ({ children, className = "" }: { children: React.ReactNode; className?: string }) => {
  return <div className={`card-standard ${className}`}>{children}</div>;
};

export const CardHeader = ({ children, className = "" }: { children: React.ReactNode; className?: string }) => {
  return <div className={`p-6 border-b border-border ${className}`}>{children}</div>;
};

export const CardContent = ({ children, className = "" }: { children: React.ReactNode; className?: string }) => {
  return <div className={`p-6 ${className}`}>{children}</div>;
};

export const CardFooter = ({ children, className = "" }: { children: React.ReactNode; className?: string }) => {
  return <div className={`p-6 border-t border-border bg-slate-50/50 dark:bg-slate-900/50 ${className}`}>{children}</div>;
};
