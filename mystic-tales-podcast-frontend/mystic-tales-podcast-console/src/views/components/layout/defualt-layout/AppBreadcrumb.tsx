import React from "react"
import { Link, useLocation } from "react-router-dom"
import { CBreadcrumb, CBreadcrumbItem } from "@coreui/react"
import routes from "../../../../routes"

const AppBreadcrumb = () => {
  const currentLocation = useLocation().pathname

  const getRouteName = (pathname: string) => {
    let currentRoute = routes.find((route) => route.path === pathname)

    if (!currentRoute) {
      const pathParts = pathname.split("/")

      currentRoute = routes.find((route) => {
        const routeParts = route.path.split("/")

        if (routeParts.length !== pathParts.length) return false

        return routeParts.every((part, i) => part.startsWith(":") || part === pathParts[i])
      })
    }

    return currentRoute ? currentRoute.name : false
  }

  const getBreadcrumbs = (location: string) => {
    const breadcrumbs: { pathname: string; name: any; active: boolean }[] = []

    let matchedRoute: any = null

    matchedRoute = routes.find((r) => r.path === location)

    if (!matchedRoute) {
      const pathParts = location.split("/")

      matchedRoute = routes.find((route) => {
        const routeParts = route.path.split("/")

        if (routeParts.length !== pathParts.length) return false

        return routeParts.every((part, i) => part.startsWith(":") || part === pathParts[i])
      })
    }

    if (matchedRoute) {
      breadcrumbs.push({
        pathname: "/",
        name: "Home",
        active: false,
      })

      if (matchedRoute.parent) {
        const parentRoute = routes.find((r) => r.path === matchedRoute.parent)
        if (parentRoute) {
          breadcrumbs.push({
            pathname: parentRoute.path,
            name: parentRoute.name,
            active: false,
          })
        }
      }

      breadcrumbs.push({
        pathname: location,
        name: matchedRoute.name,
        active: true,
      })
    } else {
      location.split("/").reduce((prev, curr, index, array) => {
        const currentPathname = `${prev}/${curr}`

        if (curr) {
          const name = getRouteName(currentPathname)
          if (name) {
            breadcrumbs.push({
              pathname: currentPathname,
              name,
              active: index + 1 === array.length,
            })
          }
        }

        return currentPathname
      })
    }

    return breadcrumbs
  }

  const breadcrumbs = getBreadcrumbs(currentLocation)
  //console.log("Breadcrumbs:", breadcrumbs)
  return (
    <CBreadcrumb className="app-breadcrumb my-0">
      {breadcrumbs.map((breadcrumb, index) => {
        return (
          <CBreadcrumbItem
            className="app-breadcrumb__item"
            active={breadcrumb.active}
            key={index}
          >
            {breadcrumb.active ? (
              breadcrumb.name
            ) : (
              <Link to={breadcrumb.pathname}>{breadcrumb.name}</Link>
            )}
          </CBreadcrumbItem>
        )
      })}
    </CBreadcrumb>
  )
}

export default React.memo(AppBreadcrumb)
