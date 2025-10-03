type Props = {
    message: string;
};

const Error = ({ message }: Props) => {
    return <div className="px-5 py-2 bg-[red] text-white">{message}</div>;
};

export default Error;
