import React from 'react';
import './styles.scss';
const Loading2 = ( {title}: {title: string}) => {
    return (
        <div className="loader">
            <div className="loading-text text-md">
                {title}<span className="dot">.</span><span className="dot">.</span><span className="dot">.</span>
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

export default Loading2;