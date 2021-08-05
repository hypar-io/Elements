const NotionPageToHtml = require('notion-page-to-html');
const fs = require('fs');

const pages = {
  sl: 'https://www.notion.so/srnli/Scratch-paper-test-1bb8f53bee3b438bbaebc70334e296a9',
  // grasshopperV2: 'https://www.notion.so/hyparaec/Grasshopper-Functions-2-0-d86252d7d4944b72a0e816eba7e3ed66'
}

Object.entries(pages).forEach(([name, url]) => {
  // using then
  NotionPageToHtml
    .convert(url)
    .then((page) => {
      fs.writeFile(`./synced/${name}.json`, JSON.stringify(page), err => {
        if (err) {
          console.log('Error writing', name, 'json');
          console.error(err)
          return
        }
        // fs.writeFile(`./synced/${name}.html`, page.html, err => {
        //   if (err) {
        //     console.log('Error writing', name, 'html');
        //     console.error(err)
        //     return
        //   }
          console.log('Successfully synced', name);
        // });
      })
    });


});


