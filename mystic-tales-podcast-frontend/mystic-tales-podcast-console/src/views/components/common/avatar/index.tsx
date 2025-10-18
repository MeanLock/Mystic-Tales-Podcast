import React, { type FC } from 'react';
import styled from 'styled-components';

interface AvatarInputProps {
  src: string;
  size?: number;
}

interface InputProps {
  size?: number;
}

const Input = styled.div<InputProps>`
  position: relative;
  align-self: center;
  
  img {
    width: ${props => props.size ? `${props.size}px` : '186px'};
    height: ${props => props.size ? `${props.size}px` : '186px'};
    object-fit: cover;
    border-radius: 50%;
  }
  
  .circle {
    width: ${props => props.size ? `${props.size}px` : '186px'};
    height: ${props => props.size ? `${props.size}px` : '186px'};
    border-radius: 50%;
  }
  
  label {
    position: absolute;
    width: 48px;
    height: 48px;
    background: #312e38;
    border-radius: 50%;
    right: 0;
    bottom: 0;
    border: 0;
    cursor: pointer;
    transition: background-color 0.2s;
    display: flex;
    align-items: center;
    justify-content: center;
    
    input {
      display: none;
    }
    
    svg {
      width: 20px;
      height: 20px;
      color: #f4ede8;
    }
    
    &:hover {
      background: #007bff;
    }
  }
`;

const AvatarInput: FC<AvatarInputProps> = ({ src, size }) => {
   return (
    <Input size={size}>
      <img src={src} alt="Avatar" />
    </Input>
  );
};

export default AvatarInput;
export type { AvatarInputProps };