function tabResponsive()
{
    let magiTabRespo = document.getElementsByClassName('magiTableResponsive').length;    

    for (var a = 0; a < magiTabRespo; a++)
    {
        let magiTab = document.getElementsByClassName('magiTableResponsive')[a]

        let tableLenght = magiTab.getElementsByTagName('table')

        tableLenght[0].children[0].classList.add('hide_thead');


        let lenthTh = tableLenght[0].querySelectorAll('thead').getElementsByTagName('th')

        let lengthTr = tableLenght[0].querySelectorAll('tbody').getElementsByTagName('tr')

        tabResponsiveInsideFun(lenthTh, lengthTr);
    }
   
}

function tabResponsiveInsideFun(lenthTh, lengthTr)
{
    for (var i = 0; i < lengthTr.length; i++)
    {

        for (var j = 0; j < lenthTh.length; j++)
        {

            let thText = lenthTh[j].textContent;
            let strongTag = document.createElement('strong');
            strongTag.classList.add('magi-table-thead');
            let text = document.createTextNode(thText);

            strongTag.appendChild(text)

            if (lengthTr[i].children[0].getAttribute('colspan'))
            {

                lengthTr[i].children[0].classList.add('paddingLeft0');
            } else
            {
                lengthTr[i].classList.add('trPaddingOn');
                lengthTr[i].querySelectorAll('td')[j].classList.add('paddingON');
                lengthTr[i].querySelectorAll('td')[j].appendChild(strongTag);
            }

        }
    }
}

tabResponsive();
