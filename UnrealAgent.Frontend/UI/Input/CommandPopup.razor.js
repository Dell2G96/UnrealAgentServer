
export function scrollToItem(prefix, index)
{
    const el = document.getElementById(prefix + '-' + index);
    if(el)
        el.scrollIntoView({behavior: 'smooth', block:'nearest'});
}