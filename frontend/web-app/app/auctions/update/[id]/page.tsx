import { getDetailedViewData } from "@/app/actions/auctionActions";
import Heading from "@/app/components/Heading";
import AuctionForm from "../../AuctionForm";

export default async function Update({params}: {params: Promise<{id: string}>}) {    
  const {id} = await params;
  const data = await getDetailedViewData(id);

  return (
    <div className="mx-auto max-w- to-75% shadow-lg p-10 bg-white rounded-lg">
      <Heading title= "Update your Auction" subtitle="Please update the details of your
       car (only these auction propertie can be updated)"/>
       <AuctionForm auction={data}/>
    </div>
  )
}
