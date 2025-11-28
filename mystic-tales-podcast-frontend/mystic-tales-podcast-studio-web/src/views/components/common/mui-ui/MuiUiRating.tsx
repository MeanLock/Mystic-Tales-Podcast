import * as React from 'react';
import Rating from '@mui/material/Rating';
import Box from '@mui/material/Box';
import StarIcon from '@mui/icons-material/Star';

const labels: { [index: string]: string } = {
    1: 'Ok',
    2: 'Good',
    3: 'Excellent'
};

function getLabelText(value: number) {
    return `${value} Star${value !== 1 ? 's' : ''}, ${labels[value]}`;
}

export function HoverRating() {
    const [value, setValue] = React.useState<number | null>(2);
    const [hover, setHover] = React.useState(-1);

    return (
        <Box sx={{ width: 200, display: 'flex', alignItems: 'center' }}>
            <Rating
                name="hover-feedback"
                value={value}
                precision={0.5}
                getLabelText={getLabelText}
                onChange={(event, newValue) => {
                    setValue(newValue);
                }}
                onChangeActive={(event, newHover) => {
                    setHover(newHover);
                }}
                emptyIcon={<StarIcon style={{ opacity: 0.55 }} fontSize="inherit" />}
            />
            {value !== null && (
                <Box sx={{ ml: 2 }}>{labels[hover !== -1 ? hover : value]}</Box>
            )}
        </Box>
    );
}

export function StaticRating({ rate }) {
    const [value, setValue] = React.useState<number | null>(2);

    return (
        <Box sx={{ width: 200, display: 'flex', alignItems: 'center' }}>
            <Rating
                readOnly
                name="hover-feedback"
                value={rate}
                precision={0.5}
                getLabelText={getLabelText}
                emptyIcon={<StarIcon style={{ opacity: 0.55 }} fontSize="inherit" />}
                max={3}
            />
            {/* {rate !== null && (
          <Box sx={{ ml: 2 }}>{labels[hover !== -1 ? hover : value]}</Box>
        )} */}
        </Box>
    );
}