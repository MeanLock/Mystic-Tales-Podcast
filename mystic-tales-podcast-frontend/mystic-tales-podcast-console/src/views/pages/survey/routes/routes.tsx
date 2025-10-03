import {
    EndSurveyCustomer,
    MySurvey,
    SurveyCustomer,
    SurveyEdit,
    SurveyNew,
    SurveyShare,
} from "./index";

export const routes = [
    // {
    //     name: "TODo",
    //     path: "/",
    //     element: <ToDo />,
    //     requiresAuth: false,
    // },
 
    {
        name: "SurveyNew",
        path: "/survey/new",
        element: <SurveyNew />,
        requiresAuth: false,
    },
    {
        name: "MySurvey",
        path: "/survey/manage",
        element: <MySurvey />,
        requiresAuth: false,
    },
    {
        name: "MySurveyShare",
        path: "/survey/:id/share", //Thịnh, đổi đường dẫn từ /survey/share/:id sang /survey/:id/share
        element: <SurveyShare />,
        requiresAuth: false,
    },
    {
        name: "SurveyEdit",
        path: "/survey/:id/editing", //Thịnh, đổi đường dẫn từ /survey/update/:id sang /survey/:id/editing
        element: <SurveyEdit />,
        requiresAuth: false,
    },
    {
        name: "SurveyCustomer",
        path: "/:id/taking", //Thịnh, đổi đường dẫn từ /survey/taking/:id sang /survey/:id/taking
        element: <SurveyCustomer />,
        requiresAuth: false,
    },
     {
        name: "EndSurveyCustomer",
        path: "/survey/:id/end", //Thịnh, đổi đường dẫn từ /survey/end/:id sang /survey/:id/end
        element: <EndSurveyCustomer />,
        requiresAuth: false,
    },
] as const;

type RouteName = (typeof routes)[number]["name"];

type RoutesMap = {
    [K in RouteName]: (typeof routes)[number]["path"];
};

export const routesMap = ((): RoutesMap => {
    return routes.reduce((acc, route) => {
        acc[route.name] = route.path;
        return acc;
    }, {} as RoutesMap);
})();
