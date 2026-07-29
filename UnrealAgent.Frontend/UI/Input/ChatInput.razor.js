
export function setupEnterSubmit(textarea) 
{
    textarea.addEventListener("keydown", function (e)
    {
        if(e.key === "Enter" && !e.shiftKey)
        {
            e.preventDefault();
            textarea.closest("form").requestSubmit();
        }
    });
}