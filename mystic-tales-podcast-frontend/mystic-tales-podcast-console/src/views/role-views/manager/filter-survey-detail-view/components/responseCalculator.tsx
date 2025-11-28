interface ApiResponse {
  Id: number
  SurveyTakenResultId: number
  SurveyQuestionId: string
  IsValid: boolean
  ValueJsonString: string
}

interface Question {
  Id: string
  SurveyId: number
  QuestionTypeId: number
  Content: string
  Description: string | null
  TimeLimit: number
  IsVoiced: boolean
  Order: number
  ConfigJsonString: string | null
  DeletedAt: string | null
  Version: number
  MainImageUrl: string | null
  Options?: Array<{
    Id: string
    SurveyQuestionId: string
    Content: string
    Order: number
    MainImageUrl: string | null
  }>
}

interface QuestionResponseSummary {
  Questions: Question[]
  Responses: ApiResponse[]
}

// Updated function to accept questions from context and summaryLists for responses
export const calculateResponseData = (contextQuestions: Question[], summaryLists: QuestionResponseSummary[]) => {
  const processedData: { [key: string]: any } = {}

  // Get all responses from summaryLists
  const allResponses: ApiResponse[] = []
  summaryLists.forEach((summary) => {
    allResponses.push(...summary.Responses)
  })

  //console.log("allResponses:", allResponses)

  // Process each question from context
  contextQuestions.forEach((question) => {
    const questionResponses = allResponses.filter(
      (response) => response.SurveyQuestionId === question.Id && response.IsValid,
    )

    const totalResponses = questionResponses.length

    //console.log(`Question ${question.Id} has ${totalResponses} responses`)

    switch (question.QuestionTypeId) {
      case 1: // Single Choice
      case 2: // Multiple Choice
        processedData[question.Id] = calculateChoiceResponses(question, questionResponses, totalResponses)
        break

      case 3: // Single Slider
      case 4: // Range Slider
        processedData[question.Id] = calculateSliderResponses(question, questionResponses, totalResponses)
        break

      case 5: // Text Input
        processedData[question.Id] = calculateTextResponses(question, questionResponses, totalResponses)
        break

      case 6: // Rating
        processedData[question.Id] = calculateRatingResponses(question, questionResponses, totalResponses)
        break

      case 7: // Ranking
        processedData[question.Id] = calculateRankingResponses(question, questionResponses, totalResponses)
        break

      default:
        processedData[question.Id] = {
          totalResponses,
          unsupported: true,
        }
    }
  })

  //console.log("Final processedData:", processedData)
  return processedData
}

const calculateChoiceResponses = (question: Question, responses: ApiResponse[], totalResponses: number) => {
  const optionCounts: { [key: number]: number } = {}
  let optionsFromResponses: any[] = []

  //console.log(`Calculating choice responses for question ${question.Id}`)

  // Get options from the first response since Questions in summaryLists have empty Options
  if (responses.length > 0) {
    try {
      const firstResponse = JSON.parse(responses[0].ValueJsonString)
      optionsFromResponses = firstResponse.QuestionContent?.Options || []
      //console.log("Options from response:", optionsFromResponses)
    } catch (error) {
      console.error("Error parsing first response for options:", error)
    }
  }

  // Use options from responses if available, otherwise use context options
  const optionsToUse = optionsFromResponses.length > 0 ? optionsFromResponses : question.Options || []

  if (optionsToUse.length === 0) {
    console.warn(`Question ${question.Id} has no options`)
    return {
      totalResponses,
      responses: [],
    }
  }

  // Initialize counts
  optionsToUse.forEach((option: any) => {
    optionCounts[option.id || option.Id] = 0
  })

  // Count responses
  responses.forEach((response) => {
    try {
      const parsedValue = JSON.parse(response.ValueJsonString)
      const questionResponse = parsedValue.QuestionResponse

      //console.log(`Processing response: parsedValue`, parsedValue)
      //console.log("Question response:", questionResponse)

      if (question.QuestionTypeId === 1) {
        // Single Choice - get from singleChoice field
        const optionId = questionResponse.SingleChoice
        if (optionId && optionCounts.hasOwnProperty(optionId)) {
          optionCounts[optionId]++
        }
      } else if (question.QuestionTypeId === 2) {
        // Multiple Choice - get from multipleChoice field (should be array)
        const selectedOptions = questionResponse.MultipleChoice
        if (Array.isArray(selectedOptions)) {
          selectedOptions.forEach((optionId) => {
            if (optionCounts.hasOwnProperty(optionId)) {
              optionCounts[optionId]++
            }
          })
        }
      }
    } catch (error) {
      console.error("Error parsing response value:", error)
    }
  })

  const processedResponses = optionsToUse.map((option: any) => ({
    optionId: option.id || option.Id,
    content: option.content || option.Content,
    count: optionCounts[option.id || option.Id],
    percentage: totalResponses > 0 ? Math.round((optionCounts[option.id || option.Id] / totalResponses) * 100) : 0,
    MainImageUrl: option.MainImageUrl || null,
  }))

  //console.log("processedResponses for question", question.Id, ":", processedResponses)

  return {
    totalResponses,
    responses: processedResponses,
  }
}

const calculateSliderResponses = (question: Question, responses: ApiResponse[], totalResponses: number) => {
  if (question.QuestionTypeId === 3) {
    // Single Slider - create distribution across the configured range
    const values: number[] = []

    // Get config from question to determine the overall range
    let configMin = 0
    let configMax = 100

    try {
      if (question.ConfigJsonString) {
        const config = JSON.parse(question.ConfigJsonString)
        configMin = config.min || 0
        configMax = config.max || 100
      }
    } catch (error) {
      console.error("Error parsing config:", error)
    }

    responses.forEach((response) => {
      try {
        const parsedValue = JSON.parse(response.ValueJsonString)
        const questionResponse = parsedValue.QuestionResponse

        const inputValue = questionResponse.Input?.Value
        if (inputValue !== null && inputValue !== undefined) {
          const numValue = typeof inputValue === "number" ? inputValue : Number.parseFloat(inputValue)
          if (!isNaN(numValue)) {
            values.push(numValue)
          }
        }
      } catch (error) {
        console.error("Error parsing single slider value:", error)
      }
    })

    if (values.length === 0) {
      return {
        totalResponses,
        averageValue: 0,
        configMin,
        configMax,
        distribution: [],
        type: "single",
      }
    }

    // Create distribution buckets across the config range
    const bucketCount = Math.min(20, configMax - configMin + 1) // Max 20 buckets or number of possible values
    const bucketSize = (configMax - configMin) / bucketCount
    const distribution: Array<{ label: string; value: number; count: number }> = []

    for (let i = 0; i < bucketCount; i++) {
      const bucketStart = configMin + bucketSize * i
      const bucketEnd = configMin + bucketSize * (i + 1)
      const bucketMid = (bucketStart + bucketEnd) / 2

      // Count values that fall in this bucket
      const count = values.filter((value) => value >= bucketStart && value < bucketEnd).length

      distribution.push({
        label: `${Math.round(bucketMid).toString()}`,
        value: bucketMid,
        count,
      })
    }

    // Calculate statistics
    const averageValue = values.reduce((sum, val) => sum + val, 0) / values.length
    const minValue = Math.min(...values)
    const maxValue = Math.max(...values)

    return {
      totalResponses,
      averageValue: Math.round(averageValue * 100) / 100,
      minValue,
      maxValue,
      configMin,
      configMax,
      distribution,
      type: "single",
    }
  } else if (question.QuestionTypeId === 4) {
    // Range Slider - create distribution across the configured range
    const rangePairs: Array<{ min: number; max: number; midpoint: number }> = []

    // Get config from question to determine the overall range
    let configMin = 0
    let configMax = 100

    try {
      if (question.ConfigJsonString) {
        const config = JSON.parse(question.ConfigJsonString)
        configMin = config.Min  || 0
        configMax = config.Max || 100
      }
    } catch (error) {
      console.error("Error parsing config:", error)
    }

    responses.forEach((response) => {
      try {
        const parsedValue = JSON.parse(response.ValueJsonString)
        const questionResponse = parsedValue.QuestionResponse

        const rangeValue = questionResponse.Range
        if (rangeValue && typeof rangeValue === "object") {
          const minValue = rangeValue.Min
          const maxValue = rangeValue.Max

          if (minValue !== null && minValue !== undefined && maxValue !== null && maxValue !== undefined) {
            const numMin = typeof minValue === "number" ? minValue : Number.parseFloat(minValue)
            const numMax = typeof maxValue === "number" ? maxValue : Number.parseFloat(maxValue)

            if (!isNaN(numMin) && !isNaN(numMax) && numMin <= numMax) {
              const midpoint = (numMin + numMax) / 2
              rangePairs.push({ min: numMin, max: numMax, midpoint })
            }
          }
        }
      } catch (error) {
        console.error("Error parsing range slider value:", error)
      }
    })

    if (rangePairs.length === 0) {
      return {
        totalResponses,
        type: "range",
        distribution: [],
        configMin,
        configMax,
      }
    }

    // Create distribution buckets across the config range
    const bucketCount = 20 // Create 20 segments for smooth curve
    const bucketSize = (configMax - configMin) / bucketCount
    const distribution: Array<{ label: string; value: number; count: number }> = []

    for (let i = 0; i < bucketCount; i++) {
      const bucketStart = configMin + bucketSize * i
      const bucketEnd = configMin + bucketSize * (i + 1)
      const bucketMid = (bucketStart + bucketEnd) / 2

      // Count responses whose midpoint falls in this bucket
      const count = rangePairs.filter((pair) => pair.midpoint >= bucketStart && pair.midpoint < bucketEnd).length

      distribution.push({
        label: `${Math.round(bucketMid).toLocaleString()}`,
        value: bucketMid,
        count,
      })
    }

    // Calculate statistics
    const minValues = rangePairs.map((r) => r.min)
    const maxValues = rangePairs.map((r) => r.max)
    const averageMinValue = minValues.reduce((sum, val) => sum + val, 0) / minValues.length
    const averageMaxValue = maxValues.reduce((sum, val) => sum + val, 0) / maxValues.length

    return {
      totalResponses,
      averageMinValue: Math.round(averageMinValue * 100) / 100,
      averageMaxValue: Math.round(averageMaxValue * 100) / 100,
      configMin,
      configMax,
      distribution,
      type: "range",
    }
  }

  return {
    totalResponses,
    type: "unknown",
  }
}

const calculateRatingResponses = (question: Question, responses: ApiResponse[], totalResponses: number) => {
  const ratingCounts: { [key: number]: number } = {
    1: 0,
    2: 0,
    3: 0,
    4: 0,
    5: 0,
  }

  let totalRating = 0
  let validRatings = 0

  responses.forEach((response) => {
    try {
      const parsedValue = JSON.parse(response.ValueJsonString)
      const questionResponse = parsedValue.QuestionResponse

      // Rating is stored in input.value
      const rating = questionResponse.Input?.Value
      const ratingNum = typeof rating === "number" ? rating : Number.parseInt(rating)

      if (ratingNum >= 1 && ratingNum <= 5) {
        ratingCounts[ratingNum]++
        totalRating += ratingNum
        validRatings++
      }
    } catch (error) {
      console.error("Error parsing rating value:", error)
    }
  })

  const averageRating = validRatings > 0 ? totalRating / validRatings : 0

  const processedResponses = Object.entries(ratingCounts).map(([rating, count]) => ({
    rating: Number.parseInt(rating),
    count,
    percentage: totalResponses > 0 ? Math.round((count / totalResponses) * 100) : 0,
  }))

  return {
    totalResponses,
    averageRating: Math.round(averageRating * 10) / 10,
    responses: processedResponses,
  }
}

const calculateTextResponses = (question: Question, responses: ApiResponse[], totalResponses: number) => {
  const textResponses = responses
    .map((response, index) => {
      try {
        const parsedValue = JSON.parse(response.ValueJsonString)
        const questionResponse = parsedValue.QuestionResponse

        // Text is stored in input.value
        const content = questionResponse.Input?.Value
        const textContent = typeof content === "string" ? content : String(content)

        return {
          id: response.Id,
          content: textContent,
          timestamp: new Date().toISOString().slice(0, 16).replace("T", " "),
        }
      } catch (error) {
        console.error("Error parsing text response:", error)
        return null
      }
    })
    .filter((response) => response !== null && response.content.trim() !== "")

  return {
    totalResponses,
    responseCount: textResponses.length,
    textResponses: textResponses.slice(0, 15), // Limit to 15 responses for display
  }
}

const calculateRankingResponses = (question: Question, responses: ApiResponse[], totalResponses: number) => {
  const optionRankings: { [key: number]: number[] } = {}
  let optionsFromResponses: any[] = []

  // Get options from the first response
  if (responses.length > 0) {
    try {
      const firstResponse = JSON.parse(responses[0].ValueJsonString)
      optionsFromResponses = firstResponse.questionContent?.options || []
    } catch (error) {
      console.error("Error parsing first response for ranking options:", error)
    }
  }

  const optionsToUse = optionsFromResponses.length > 0 ? optionsFromResponses : question.Options || []

  if (optionsToUse.length === 0) {
    console.warn(`Question ${question.Id} has no options for ranking`)
    return {
      totalResponses,
      averageRankings: [],
    }
  }

  // Initialize rankings for each option
  optionsToUse.forEach((option: any) => {
    optionRankings[option.id || option.Id] = []
  })

  responses.forEach((response) => {
    try {
      const parsedValue = JSON.parse(response.ValueJsonString)
      const questionResponse = parsedValue.QuestionResponse

      // Ranking is stored in ranking array with surveyOptionId and rankIndex
      const rankings = questionResponse.Ranking
      if (Array.isArray(rankings)) {
        rankings.forEach((rankItem: any) => {
          const optionId = rankItem.SurveyOptionId
          const rankIndex = rankItem.RankIndex

          if (optionRankings[optionId] && !isNaN(rankIndex)) {
            optionRankings[optionId].push(rankIndex)
          }
        })
      }
    } catch (error) {
      console.error("Error parsing ranking value:", error)
    }
  })

  const averageRankings = optionsToUse.map((option: any) => {
    const rankings = optionRankings[option.id || option.Id]
    const averageRank = rankings.length > 0 ? rankings.reduce((sum, rank) => sum + rank, 0) / rankings.length : 0

    return {
      optionId: option.id || option.Id,
      content: option.content || option.Content,
      averageRank: Math.round(averageRank * 10) / 10,
    }
  })

  return {
    totalResponses,
    averageRankings,
  }
}
