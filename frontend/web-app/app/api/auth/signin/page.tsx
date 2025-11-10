import EmptyFilter from "@/app/components/EmptyFilter";

export default async function SignIn({searchParams} : {searchParams: {callbackUrl: string}}) {
    const params = await searchParams;

    return (
    <EmptyFilter
        title="You need to be logged in to do that"
        subtitle="Please click below to login"
        showLogin
        callbackUrl={params.callbackUrl}
     />
  )
}
