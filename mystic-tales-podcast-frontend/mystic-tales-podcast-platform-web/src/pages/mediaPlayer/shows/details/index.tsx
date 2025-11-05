import { useParams } from "react-router-dom";

const ShowDetailsPage = () => {
  const { id } = useParams();
  return (
    <div>
      <h1>Show Details {id}</h1>
    </div>
  );
};

export default ShowDetailsPage;
