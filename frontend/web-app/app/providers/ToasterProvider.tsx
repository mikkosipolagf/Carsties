"use client";
import React from "react";
import { Toaster } from "react-hot-toast";

const ToasterProvider: React.FC = () => {
  return <Toaster position="bottom-right" />;
};

export default ToasterProvider;
