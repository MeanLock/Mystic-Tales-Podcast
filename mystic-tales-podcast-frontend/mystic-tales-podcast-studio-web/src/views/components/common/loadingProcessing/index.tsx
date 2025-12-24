import React from 'react';
import './styles.scss';
const LoadingProcessing = () => {
    return (
        <div className="loader">
            <div className="loading-text text-md">
                Your episode audio is being processed, it will be available soon<span className="dot">.</span><span className="dot">.</span><span className="dot">.</span>
            </div>
            <div className="loading-bar-background">
                <div className="loading-bar">
                    <div className="white-bars-container">
                        <div className="white-bar"></div>
                        <div className="white-bar"></div>
                        <div className="white-bar"></div>
                        <div className="white-bar"></div>
                        <div className="white-bar"></div>
                        <div className="white-bar"></div>
                        <div className="white-bar"></div>
                        <div className="white-bar"></div>
                        <div className="white-bar"></div>
                        <div className="white-bar"></div>
                    </div>
                </div>
            </div>
        </div>

    );
};

export default LoadingProcessing;