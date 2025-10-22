const path = require("path");

module.exports = {
  style: {
    sass: {
      loaderOptions: {
        additionalData: `
          @use "src/styles/variables/app-variables" as app_vars;
          @use "src/styles/variables/FunctionArea/function-side-bar-variables" as functionSideBar_vars;
          @use "src/styles/variables/MainArea/chat-box-variables" as chatBox_vars;
          @use "src/styles/variables/MainArea/chat-box-info-variables" as chatBoxInfo_vars;
          @use "src/styles/variables/MainArea/room-browser-variables" as roomBrowser_vars;
        `,
      },
    },
  },
};
